using Ionic.Zip;

namespace Functions;
public static class Zipper
{
    [FunctionName(nameof(Zipper))]
    public static async Task Run(
        [ServiceBusTrigger(Constants.Queues.BatchedQueue, Connection = "ServiceBusConnection")]
            QueueBatchItem queueItem,
            Int32 deliveryCount,
            DateTime enqueuedTimeUtc,
            string messageId,
        [ServiceBus(Constants.Queues.ZipsQueue, Connection = "ServiceBusConnection")] IAsyncCollector<QueueBatchItem> queue,
        [Blob(Constants.Storage.FailedContainer, Connection = "AzureWebJobsFTPStorage")] BlobContainerClient clientError,
        [Blob($"{Constants.Storage.ZipsContainer}/{{queueItem.BatchId}}", Connection = "AzureWebJobsZipStorage")] BlockBlobClient clientZip,
        // [Sql(commandText: "GetBatchFiles", commandType: System.Data.CommandType.StoredProcedure,
        //         parameters: "@BatchId={queueItem.NameWithoutExtension}", connectionStringSetting: "SqlConnectionString")]
        //         IList<Functions.Model.File> batchDB,
        [Sql(commandText: "dbo.Files", connectionStringSetting: "SqlConnectionString")] IAsyncCollector<File> fileDb,
        [Sql(commandText: "dbo.FileLogs", connectionStringSetting: "SqlConnectionString")] IAsyncCollector<FileLog> fileLogsDb,
        ILogger logger)
    {
        logger.LogInformation($"[Zipper] Service Bus trigger function Processed blobs myQueueItem: {queueItem}");
        IList<TaggedBlobItem> blobs = new List<TaggedBlobItem>();
        IList<BatchJob> jobs = queueItem.Files.Select(f => new BatchJob(f)).ToList();

        if (jobs.Count < 1) //Must throw for the Service Bus retry mechanism
            throw new ArgumentOutOfRangeException(nameof(queueItem), $"[Zipper] No blobs found for queueItem: {queueItem}");

        // download file streams
        // await Task.WhenAll(jobs.Select(job => job.LeaseClient.AcquireAsync(LeaseDuration).ContinueWith(j => job.Lease = j.Result, TaskContinuationOptions.ExecuteSynchronously)));
        await Task.WhenAll(jobs.Select(job => job.DownloadToAsync(job.Stream)
            .ContinueWith(r => logger.LogInformation($"[Zipper] DownloadToAsync {job.Name}, length: {job.Stream.Length}, Success: {r.IsCompletedSuccessfully}, Exception: {r.Exception?.Message}"))
        ));

        logger.LogInformation($"[Zipper] Downloaded {jobs.Count} blobs. Files: {string.Join(",", jobs.Select(j => $"{j.Name} ({j.Stream.Length})"))}");
        using (System.IO.MemoryStream zipStream = new System.IO.MemoryStream())
        {
            using (ZipFile zip = new ZipFile())
            {
                foreach (BatchJob job in jobs)
                {
                    try
                    {
                        if (job.Stream == null)
                        {
                            logger.LogError($"[Zipper] Package zip Cannot compress part, no stream created: {job}");
                            job.File.Status = BlobStatus.Failed;
                            job.File.Text = "Zip failed, No stream downloaded";
                            await fileLogsDb.AddAsync(new FileLog()
                            {
                                FileName = job.File.Name,
                                Container = job.File.Container,
                                Status = BlobStatus.Failed,
                                Created = DateTime.UtcNow,
                                Data = job.Serialize(),
                                Text = "Zip entry failed, No stream downloaded"
                            });
                            await job.MoveToFailedAsync();
                            continue;
                        }

                        job.File.Status = BlobStatus.Zipped;
                        job.Stream.Position = 0;
                        zip.AddEntry(job.Name, job.Stream);
                        await fileLogsDb.AddAsync(new FileLog()
                        {
                            FileName = job.File.Name,
                            Container = job.File.Container,
                            Status = BlobStatus.Batched,
                            Created = DateTime.UtcNow,
                            Data = job.Serialize(),
                            Text = "Zip entry created"
                        });

                    }
                    catch (Exception ex)
                    {
                        logger.LogError(ex, $"[Zipper] Error in job {job.Name}, ActivityDetails: , exception: {ex.Message}");
                        job.File.Status = BlobStatus.Failed;
                        job.File.Text = $"[Zipper] Error in job {job.Name}, ActivityDetails: , exception: {ex.Message}";
                    }
                }
                zip.Save(zipStream);
            }

            logger.LogInformation($"[Zipper] Creating zip stream: {queueItem.BatchId}.zip");
            zipStream.Position = 0;
            // TODO: (Niv) Sometimes azure trigger the same batchId (right now dont know why).
            // So we check if the blob is already exists, if true we ignore this execution with the batchId.
            Response<bool> isExist = await clientZip.ExistsAsync();
            if (isExist.Value)
                logger.LogWarning($"[Zipper] Zip with batchId {queueItem.BatchId} already exists. ignoring this execution, ActivityDetails: ");

            await fileDb.AddAsync(new File()
            {
                Name = $"{queueItem.BatchId}.zip",
                Created = DateTime.UtcNow,
                Modified = DateTime.UtcNow,
                Container = Constants.Storage.ZipsContainer,
                Length = zipStream.Length,
                Status = isExist.Value ? BlobStatus.ReSaved : BlobStatus.Saved,
                TryCount = deliveryCount, // TODO: handle files with same name, delettion of old records will be needed
                Completed = false,
                Faulted = false,
                BatchId = queueItem.BatchId,
                Text = $"Zip file created, {jobs.Count} files of total of {zipStream.Length.Bytes2Megabytes()}MB zipped. Details: {queueItem.Serialize()}"
            });
        }

        await Task.WhenAll(jobs.Select(job =>
            fileDb.AddAsync(job.File)
            .ContinueWith(t => job.CompleteAsync())
        //    .ContinueWith(t => job.LeaseClient.ReleaseAsync())
        ));

        logger.LogInformation($"[Zipper] files DELETED/MOVED {jobs.Count} blobs. Files: {string.Join(",", jobs.Select(t => $"{t.Name} ({t.File.Length.Bytes2Megabytes()}MB)"))}");
        logger.LogInformation($"[Zipper] Zip file completed, post creation marking blobs. Details: {queueItem}");
    }
}
