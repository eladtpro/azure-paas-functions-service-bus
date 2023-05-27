namespace Functions.Core;
[Flags]
public enum BlobStatus
{
    New = 0,
    Pending = 1,
    Batched = 2,
    Zipped = 4,
    Deleted = 8,
    Failed = 16,
    Saved  = 32,
    ReSaved  = 64,
    Distributed = 128
}
