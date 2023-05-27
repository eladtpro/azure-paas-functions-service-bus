namespace Functions.Model;

public class FileLog
{
    public int Id { get; set; }
    public string FileName { get; set; }
    public string Container { get; set; }
    public DateTime Created { get; set; }
    public BlobStatus Status { get; set; }
    public string Data { get; set; }
    public string Text { get; set; }

    public override string ToString()
    {
        return $"Id: {Id}, FileName: {FileName}, Container: {Container}, Created: {Created}, Status: {Status}, Data: {Data}, Text: {Text}";
    }
}
