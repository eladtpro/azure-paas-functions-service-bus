namespace Functions.Core;

public static class Constants
{
    public static class Storage
    {
        // IMPORTANT: When changing ContainerName make sure to change EventGrid's topic filter
        public const string NewContainer = "new";
        public const string PendingContainer = "pending";
        public const string FailedContainer = "failed";
        public const string BatchedContainer = "batched";
        // public const string ZippedContainer = "zipped";
        public const string ZipsContainer = "zips";
    }

    public static class Queues
    {
        public const string PendingQueue = "pending";
        public const string BatchedQueue = "batched";
        public const string ZipsQueue = "zips";
    }

    public static class Tables
    {
        public const string Files = "Files";
        public const string FileLogs = "FileLogs";
    }
}
