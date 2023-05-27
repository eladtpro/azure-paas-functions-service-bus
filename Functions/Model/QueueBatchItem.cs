namespace Functions.Model
{
    // this is the class that Collector inserts into the queue, to be consumed by the zipper function
    public class QueueBatchItem
    {
        public string Namespace { get; set; } = "default";
        public string BatchId { get; set; }
        public File[] Files {get;set; }

        override public string ToString()
        {
            return $"BatchId: {BatchId}, Namespace: {Namespace}, Files: {Files.Serialize()}";
        }
    }
}
