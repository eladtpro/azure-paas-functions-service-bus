namespace Functions;

public class BlobsCleaner
{
 
    [FunctionName(nameof(BlobsCleaner))]
    public async Task Run(
        [TimerTrigger("0 */%BlobsDeleteTimer% * * * *")] TimerInfo myTimer,
        [Blob(Constants.Storage.ZipsContainer, Connection = "AzureWebJobsFTPStorage")] BlobContainerClient blobContainerClient,
        [Sql(commandText: "GetLeftoverFiles", commandType: System.Data.CommandType.StoredProcedure,
                parameters: "@Count=50, @Status={(int)BlobStatus.Zipped}, @ThresholdHours=24", connectionStringSetting: "SqlConnectionString")]
                IList<Functions.Model.File> leftoverDB,
        [Sql(commandText: "dbo.Files", connectionStringSetting: "SqlConnectionString")] IAsyncCollector<File> fileDb,
        [Sql(commandText: "dbo.FileLogs", connectionStringSetting: "SqlConnectionString")] IAsyncCollector<FileLog> fileLogsDb,
        ILogger log)
    {
        

        log.LogInformation($"[BlobsCleaner] Start delete zipped blobs at: {DateTime.Now}");

        await Parallel.ForEachAsync(leftoverDB,
        async (f, _) =>
        {
            await blobContainerClient.DeleteBlobIfExistsAsync(f.Name); // TODO: delete by container
            f.Status = BlobStatus.Deleted;
            f.Modified = DateTime.UtcNow;
            await fileLogsDb.AddAsync(new FileLog()
            {
                FileName = f.Name,
                Container = f.Container,
                Status = f.Status,
                Created = DateTime.UtcNow,
                Text = "File deleted by BlobsCleaner"
            });
            await fileDb.AddAsync(f);
            log.LogInformation($"[BlobsCleaner deteted {f.Name} at: {DateTime.Now}");
        });

        log.LogInformation($"[BlobsCleaner] {leftoverDB.Count} zipped blobs deleted, Details: {leftoverDB.Serialize()}");
        
        // // TODO: reafactor FindBlobsByTagsAsync for reuse
        // log.LogInformation($"[BlobsCleaner] Start Retry Batched files at blobs: {DateTime.Now}");
        // IDictionary<string, string> batches = new Dictionary<string, string>();
        // string query = $"Status = 'Batched' AND Modified < '{DateTime.UtcNow.Subtract(BatchedBlobsRetryThreshold).ToFileTimeUtc()}'";
        // List<TaggedBlobItem> blobs = new List<TaggedBlobItem>();
        // await foreach (TaggedBlobItem taggedBlobItem in blobContainerClient.FindBlobsByTagsAsync(query))
        // {
        //     BlobTags tag = new BlobTags(taggedBlobItem);
        //     batches[tag.BatchId] = tag.Namespace; 
        // }

        // log.LogInformation($"[BlobsCleaner] batches count: {batches.Count}");
        // foreach (KeyValuePair<string, string> kvp in batches)
        // {
        //     log.LogInformation($"[BlobsCleaner] Retry Batches: {DateTime.Now}, BatchId: {kvp.Key}, Namespace: {kvp.Value}");
        //     var activity = new ActivityAction() { Namespace = kvp.Value , BatchId = kvp.Key };
        //     await starter.StartNewAsync(nameof(ZipperOrchestrator), activity);
        //     log.LogInformation($"[BlobsCleaner] Namespace: {kvp.Value} and BatchId: {kvp.Key} Were Batched blobs started successfully");
        // }
        // log.LogInformation($"[BlobsCleaner] Batches count: {batches.Count} strated successfully");
    }
}

