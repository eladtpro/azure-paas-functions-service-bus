namespace Functions.Model;

public class File
{
    public string Name { get; set; }
    public string Container { get; set; }
    public DateTime Created { get; set; }
    public DateTime Modified { get; set; }
    public long Length { get; set; }
    public BlobStatus Status { get; set; }
    public string Namespace { get; set; }
    public string BatchId { get; set; }
    public string Text { get; set; }
    public int TryCount { get; set; }
    public bool Completed { get; set; }
    public bool Faulted { get; set; }

    override public string ToString()
    {
        return $"{Name}, BatchId: {BatchId}, Container: {Container}, Completed: {Completed}, Faulted: {Faulted} ({Length.Bytes2Megabytes()}MB) - {Status}, {TryCount} tries, {Text}";
    }
}