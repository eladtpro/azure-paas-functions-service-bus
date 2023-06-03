namespace Functions;

using File = Model.File;

public class BlobListener
{
    [FunctionName(nameof(BlobListener))]
    public async Task Run(
        [ServiceBusTrigger(Constants.Queues.NewQueue, Connection = "ServiceBusConnection", AutoCompleteMessages=true)]
            QueueEventGridItem queueItem,
            Int32 deliveryCount,
            DateTime enqueuedTimeUtc,
            string messageId,
        [Blob("{Path}", System.IO.FileAccess.ReadWrite, Connection = "AzureWebJobsFTPStorage")] BlockBlobClient clientNew,
        [Blob($"{Constants.Storage.PendingContainer}/{{Name}}", System.IO.FileAccess.Write, Connection = "AzureWebJobsFTPStorage")] BlockBlobClient clientPending,
        [Sql(commandText: "dbo.Files", connectionStringSetting: "SqlConnectionString")] IAsyncCollector<File> fileDb,
        [Sql(commandText: "dbo.FileLogs", connectionStringSetting: "SqlConnectionString")] IAsyncCollector<FileLog> fileLogsDb,
        ILogger logger)
    {
        logger.LogInformation($"[BlobListener] Function triggered on Service Bus Queue. queueItem.Path: {queueItem.Path}, queueItem: {queueItem}, deliveryCount: {deliveryCount} enqueuedTimeUtc: {enqueuedTimeUtc}, messageId: {messageId}");
        File file = new File()
        {
            Name = clientNew.Name,
            Created = DateTime.UtcNow,
            Modified = DateTime.UtcNow,
            Container = clientNew.BlobContainerName,
            Length = -1,
            Status = BlobStatus.New,
            TryCount = deliveryCount, // TODO: handle files with same name, delettion of old records will be needed
            Completed = false,
            Faulted = false
        };
        // Flush immidiatlly file details to DB for tracking the first time
        await fileDb.AddAsync(file);
        await fileDb.FlushAsync();

        logger.LogInformation($"[BlobListener] first record was registered: {clientNew.Name}, deliveryCount: {deliveryCount}");
        BlobProperties props = await clientNew.GetPropertiesAsync();
        await fileLogsDb.AddAsync(new FileLog()
        {
            FileName = clientNew.Name,
            Container = clientNew.BlobContainerName,
            Status = BlobStatus.New,
            Created = DateTime.UtcNow,
            Data = props.Serialize(),
        });

        file.Length = props.ContentLength;
        file.Created = props.CreatedOn.UtcDateTime;
        file.Modified = props.LastModified.UtcDateTime;
        file.Namespace = GetBlobNamespace(clientNew.Name);
        // HACK: temp comment
        //await fileDb.AddAsync(file);

        logger.LogInformation($"[BlobListener] Starting copy {clientNew.Uri} to {clientPending.Uri}");
        bool success = await clientNew.MoveToAsync(clientPending);
        await fileLogsDb.AddAsync(new FileLog()
        {
            FileName = clientNew.Name,
            Container = (success) ? clientPending.BlobContainerName : clientNew.BlobContainerName,
            Status = (success) ? BlobStatus.Pending : BlobStatus.Failed,
            Created = DateTime.UtcNow,
            Data = props.Serialize(),
            Text = (success) ? "Blob copy completed successfully." : "Blob copy operation failed."
        });
        await fileDb.AddAsync(file);

        //TODO: [SCAVENGER] check if we need to move the blob from the error container by testing the queue delivery count, also add a new log record
        if (!success) //must throw for to queue the message again
            throw new Exception($"[BlobListener] Blob copy operation failed. {clientNew.Name}");

        logger.LogInformation($"[BlobListener] BlobTags saved for blob {clientNew.Name}, Properties: {props.Serialize()}");
    }

    private static string GetBlobNamespace(string blobName)
    {
        blobName = System.IO.Path.GetFileNameWithoutExtension(blobName);

        string @namespace = blobName.Split("_").LastOrDefault();

        return (Regex.IsMatch(@namespace, "^(P|p)[1-4]$")) ? 
            Configuration.GetNamespaceVariable(@namespace.ToUpper()) :
            Configuration.GetNamespaceVariable(Configuration.DefaultBlobType);
    }
}
