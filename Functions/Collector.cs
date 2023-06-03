namespace Functions;

public class Collector
{
    private class CollectorArguments
    {
        public string Namespace { get; set; }
        public BlobContainerClient ContainerClient { get; set; }
        public IAsyncCollector<QueueBatchItem> Queue { get; set; }
        public IList<File> PendingFiles { get; set; }
        public IAsyncCollector<File> FileDb { get; set; }
        public IAsyncCollector<FileLog> FileLogsDb { get; set; }
    }

    //TODO: [SCAVENGER] Handle outdated batched blobs, look for leftover blobs that are not in the database.
    [FunctionName(nameof(Collector))]
    public async Task Run(
        [TimerTrigger("*/%CollectorTimer% * * * * *")] TimerInfo myTimer,
        [ServiceBus(Constants.Queues.BatchedQueue, Connection = "ServiceBusConnection")] IAsyncCollector<QueueBatchItem> queue,
        [Blob(Constants.Storage.PendingContainer, Connection = "AzureWebJobsFTPStorage")] BlobContainerClient containerClient,
        [Sql(commandText: "GetPendingFiles", commandType: System.Data.CommandType.StoredProcedure,
                parameters: "@SizeInMB=%MaxZipsPerExecution%", connectionStringSetting: "SqlConnectionString")]
                IEnumerable<File> pendingDB,
        [Sql(commandText: "dbo.Files", connectionStringSetting: "SqlConnectionString")] IAsyncCollector<File> fileDb,
        [Sql(commandText: "dbo.FileLogs", connectionStringSetting: "SqlConnectionString")] IAsyncCollector<FileLog> fileLogsDb,
        ILogger log)
    {
        IList<File> pending = pendingDB.ToList();

        log.LogInformation($"[Collector] executed at: {DateTime.Now} ");
        await Task.WhenAll(Configuration.Namespaces.Select(@namespace => CollectorRun(new CollectorArguments()
        {
            Namespace = @namespace,
            ContainerClient = containerClient,
            Queue = queue,
            PendingFiles = pending.Where(f => f.Namespace == @namespace).ToList(),
            FileDb = fileDb,
            FileLogsDb = fileLogsDb
        }, log)));
    }

    private async Task CollectorRun(CollectorArguments args, ILogger log)
    {
        List<File> batch = new List<File>();
        long totalSize = 0;
        log.LogInformation($"[Collector {args.Namespace}] start run for namespace: {args.Namespace}, found {args.PendingFiles.Count} pending files.");

        foreach (File file in args.PendingFiles)
        {
            batch.Add(file);
            totalSize = batch.Sum(f => f.Length);
            if (totalSize.Bytes2Megabytes() < Configuration.ZipBatchMaxSizeMB)
                continue;
            await QueueBatch(args, batch, log);

            batch.Clear();
        }

        // Handle leftovers, if more than 10MB
        totalSize = batch.Sum(f => f.Length);
        if (totalSize.Bytes2Megabytes() >= Configuration.ZipBatchMinSizeMB)
            await QueueBatch(args, batch, log);
    }

    private async Task QueueBatch(CollectorArguments args, List<File> batch, ILogger log)
    {
        long totalSize = batch.Sum(f => f.Length);
        log.LogInformation($"[Collector] {args.Namespace} found {batch.Count} blobs in total size of {totalSize.Bytes2Megabytes()}MB(/{Configuration.ZipBatchMinSizeMB}MB).\n {string.Join(",", batch.Select(t => $"{t.Name} ({t.Length.Bytes2Megabytes()}MB)"))}");
        string batchId = ActivityAction.CreateBatchId(args.Namespace, batch.Count);
        await Parallel.ForEachAsync(batch,
        async (f, _) =>
        {
            f.Status = BlobStatus.Batched;
            f.BatchId = batchId;
            f.Modified = DateTime.UtcNow;
            await args.FileDb.AddAsync(f);
            await args.FileLogsDb.AddAsync(new FileLog()
            {
                FileName = f.Name,
                Container = f.Container,
                Status = f.Status,
                Created = DateTime.UtcNow,
                Data = batchId.Serialize(),
                Text = "File marked for batch"
            });
            log.LogInformation($"[Collector {args.Namespace}] File marked {f.Name} blobs.\n BatchId: {batchId} \n TotalSize: {totalSize}.\nFiles: {string.Join(",", batch.Select(t => $"{t.Name} ({t.Length.Bytes2Megabytes()}MB)"))}");
        });

        QueueBatchItem queueItem = new QueueBatchItem() { Namespace = args.Namespace, BatchId = batchId, Files = batch.ToArray() };
        await args.Queue.AddAsync(queueItem);
    }
}
