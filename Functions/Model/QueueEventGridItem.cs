namespace Functions.Model;

// According to the schema of Microsoft.Storage.BlobCreated event: 
// https://learn.microsoft.com/en-us/azure/event-grid/event-schema-blob-storage?tabs=event-grid-event-schema#microsoftstorageblobcreated-event
[DataContract]
public class QueueEventGridItem
{
    public string Path => ExtractBlobPath();

    public string Name => ExtractBlobName();

    public string NameWithoutExtension => System.IO.Path.GetFileNameWithoutExtension(Name);

    [JsonProperty("topic", NullValueHandling = NullValueHandling.Ignore)]
    public string Topic { get; set; }

    [JsonProperty("subject", NullValueHandling = NullValueHandling.Ignore)]
    public string Subject { get; set; }

    [JsonProperty("eventType", NullValueHandling = NullValueHandling.Ignore)]
    public string EventType { get; set; }

    [JsonProperty("eventTime", NullValueHandling = NullValueHandling.Ignore)]
    public DateTime EventTime { get; set; }

    [JsonProperty("id", NullValueHandling = NullValueHandling.Ignore)]
    public string Id { get; set; }

    [JsonProperty("data", NullValueHandling = NullValueHandling.Ignore)]
    public ItemData Data { get; set; }

    [JsonProperty("dataVersion", NullValueHandling = NullValueHandling.Ignore)]
    public string DataVersion { get; set; }

    [JsonProperty("metadataVersion", NullValueHandling = NullValueHandling.Ignore)]
    public string MetadataVersion { get; set; }
    // Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse);
    public class ItemData
    {
        [JsonProperty("api", NullValueHandling = NullValueHandling.Ignore)]
        public string Api { get; set; }

        [JsonProperty("clientRequestId", NullValueHandling = NullValueHandling.Ignore)]
        public string ClientRequestId { get; set; }

        [JsonProperty("requestId", NullValueHandling = NullValueHandling.Ignore)]
        public string RequestId { get; set; }

        [JsonProperty("eTag", NullValueHandling = NullValueHandling.Ignore)]
        public string ETag { get; set; }

        [JsonProperty("contentType", NullValueHandling = NullValueHandling.Ignore)]
        public string ContentType { get; set; }

        [JsonProperty("contentLength", NullValueHandling = NullValueHandling.Ignore)]
        public int ContentLength { get; set; }

        [JsonProperty("blobType", NullValueHandling = NullValueHandling.Ignore)]
        public string BlobType { get; set; }

        [JsonProperty("url", NullValueHandling = NullValueHandling.Ignore)]
        public string Url { get; set; }

        [JsonProperty("sequencer", NullValueHandling = NullValueHandling.Ignore)]
        public string Sequencer { get; set; }
    }
    private string ExtractBlobPath()
    {
        const string PREFIX = ".blob.core.windows.net/";
        int index = this.Data.Url.LastIndexOf(PREFIX);
        if (index < PREFIX.Length)
            throw new ArgumentException("Invalid EventGrid Schema, https://learn.microsoft.com/en-us/azure/event-grid/event-schema", "url");
        index += PREFIX.Length;
        return this.Data.Url.Substring(index);
    }

    private string ExtractBlobName()
    {
        string blobPath = ExtractBlobPath();
        int index = blobPath.IndexOf('/');
        if (index < 0)
            throw new ArgumentException("Invalid EventGrid Schema, https://learn.microsoft.com/en-us/azure/event-grid/event-schema", "url");
        return blobPath.Substring(index + 1);
    }

    override public string ToString()
    {
        return this.Serialize();
    }
}
