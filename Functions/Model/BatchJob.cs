using System.IO;

namespace Functions.Model;

public class BatchJob
{
    private readonly Lazy<BlockBlobClient> client;
    // private readonly Lazy<BlockBlobClient> clientZipped;
    private readonly Lazy<BlockBlobClient> clientError;
    // private readonly Lazy<BlobLeaseClient> leaseClient;
    private readonly Lazy<MemoryStream> stream;
    public string Name => File.Name;
    // public BlobLeaseClient LeaseClient => leaseClient.Value;
    public Stream Stream => stream.Value;
    public Model.File File { get; set; }

    public BatchJob(Model.File file)
    {
        this.File = file;
        client = new Lazy<BlockBlobClient>(() => new BlockBlobClient(Configuration.AzureWebJobsFTPStorage, this.File.Container, this.File.Name));
        // clientZipped = new Lazy<BlockBlobClient>(() => new BlockBlobClient(Configuration.AzureWebJobsFTPStorage, Constants.Storage.ZippedContainer, this.File.Name));
        clientError = new Lazy<BlockBlobClient>(() => new BlockBlobClient(Configuration.AzureWebJobsFTPStorage, Constants.Storage.FailedContainer, this.File.Name));
        // leaseClient = new Lazy<BlobLeaseClient>(() => BlobClient.GetBlobLeaseClient());
        stream = new Lazy<MemoryStream>(() => new MemoryStream());
    }

    public Task<Response> DownloadToAsync(Stream destination) => client.Value.DownloadToAsync(destination);

    public Task<BlobStatus> CompleteAsync() => // TODO: decide if delete or move to zipped container
        client.Value.DeleteIfExistsAsync()
        .ContinueWith(r => File.Status = r.IsCompletedSuccessfully ? BlobStatus.Failed : BlobStatus.Deleted);
    // client.Value.MoveToAsync(clientZipped.Value);
    public Task<bool> MoveToFailedAsync() => client.Value.MoveToAsync(clientError.Value);

    public override string ToString()
    {
        return $"{File.Name}, BatchId: {File.BatchId}, Container: {File.Container}, Completed: {File.Completed}, Faulted: {File.Faulted} ({File.Length.Bytes2Megabytes()}MB) - {File.Status}, {File.TryCount} tries, {File.Text}";
    }
}