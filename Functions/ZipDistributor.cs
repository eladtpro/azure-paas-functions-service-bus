namespace Functions;

public class ZipDistributor
{
    [FunctionName(nameof(ZipDistributor))]
    public static async Task Run(
        [ServiceBusTrigger(Constants.Queues.ZipsQueue, Connection = "ServiceBusConnection", AutoCompleteMessages=true)]
            QueueEventGridItem queueItem,
            Int32 deliveryCount,
            DateTime enqueuedTimeUtc,
            string messageId,
        [Blob("{queueItem.Path}", System.IO.FileAccess.ReadWrite, Connection = "AzureWebJobsFTPStorage")] BlockBlobClient clientZip,
        [Sql(commandText: "GetBatchFiles", commandType: System.Data.CommandType.StoredProcedure,
                parameters: "@BatchId={queueItem.NameWithoutExtension}", connectionStringSetting: "SqlConnectionString")]
                IList<Functions.Model.File> batchDB,
        [Sql(commandText: "dbo.Files", connectionStringSetting: "SqlConnectionString")] IAsyncCollector<File> fileDb,
        [Sql(commandText: "dbo.FileLogs", connectionStringSetting: "SqlConnectionString")] IAsyncCollector<FileLog> fileLogsDb,
        [DurableClient] IDurableEntityClient client,
        ILogger log)
    {
        log.LogInformation($"[ZipDistributor] Triggered Function for zip: {queueItem.Path}, InstanceId {queueItem}");
        IDictionary<string, BlobCopyInfo> results = new Dictionary<string, BlobCopyInfo>();
        File file = batchDB.FirstOrDefault(f => f.Name == queueItem.Name);
        foreach (var distributionTarget in Configuration.DistributionTargets)
        {
            log.LogInformation($"[ZipDistributor] Start distribution target - {distributionTarget.TargetName}");
            string containerName = distributionTarget.ContainerName;
            if (distributionTarget.IsRoundRobin.HasValue &&
                distributionTarget.IsRoundRobin.Value &&
                distributionTarget.ContainersCount.HasValue)
            {
                log.LogInformation($"[ZipDistributor] Get container index for - {distributionTarget.TargetName}");
                var entityId = new EntityId(nameof(DurableTargetStates), distributionTarget.TargetName);

                try
                {
                    int containerNum = 0;
                    // containerNum = await context.CallEntityAsync<int>(entityId, "GetNext", distributionTarget.ContainersCount.Value);
                    EntityStateResponse<IDurableTargetState> stateResponse = await client.ReadEntityStateAsync<IDurableTargetState>(entityId);
                    containerNum = await stateResponse.EntityState.GetValue();
                    containerName += containerNum.ToString();
                    await client.SignalEntityAsync<IDurableTargetState>(entityId, proxy => proxy.MoveNext(distributionTarget.ContainersCount.Value));    
                    log.LogInformation($"[ZipDistributor] Recived container index for - {distributionTarget.TargetName} successfully. containerName: {containerName}");
                }
                catch (Exception ex)
                {
                    log.LogError(ex, $"[ZipDistributor] Failed to get container index for - {distributionTarget.TargetName}");
                }
            }
            log.LogInformation($"[ZipDistributor] Start distribute zip to distribution target - {distributionTarget.TargetName}");

            BlockBlobClient destClient = new BlockBlobClient(distributionTarget.ConnectionString, containerName,  queueItem.Name);
            Response<BlobCopyInfo> result = await clientZip.CopyToAsync(destClient);
            results.Add(distributionTarget.TargetName, result.Value);
            bool success = result.Value.CopyStatus == CopyStatus.Success;
            file.Status = success ? BlobStatus.Distributed : BlobStatus.Failed;
            file.Modified = DateTime.UtcNow;
            await fileLogsDb.AddAsync(new FileLog()
            {
                FileName = file.Name,
                Container = file.Container,
                Status = (success) ? BlobStatus.Pending : BlobStatus.Failed,
                Created = DateTime.UtcNow,
                Data = result.Value.Serialize(),
                Text = (success) ? $"Zip distributed successfully to {distributionTarget.TargetName}." : "Zip distribution operation failed."
            });
            await fileDb.AddAsync(file);

            if(success)
                log.LogInformation($"[ZipDistributor] zip {file.Name} successfully copied to {distributionTarget.TargetName}");
            else
                log.LogError($"[ZipDistributor] Failed to copy zip {queueItem.Path} to {distributionTarget.TargetName}");
        }

        if (results.Any(r => r.Value.CopyStatus != CopyStatus.Success))
        {
            string failedTargets = string.Join(",", results.Where(r => r.Value.CopyStatus != CopyStatus.Success).Select(r => r.Key));
            log.LogWarning($"[ZipDistributor] zip {queueItem.Name} not successfully copy to all destinations, failed in {failedTargets}");
        }
        else
            log.LogInformation($"[ZipDistributor] zip {queueItem.Name} successfully copy to all destinations");

        clientZip.DeleteIfExists();
        file.Status = BlobStatus.Deleted;
        file.Modified = DateTime.UtcNow;
        await fileDb.AddAsync(file);
        log.LogInformation($"[ZipDistributor] zip {queueItem.Name} deleted successfully");
    }
}
