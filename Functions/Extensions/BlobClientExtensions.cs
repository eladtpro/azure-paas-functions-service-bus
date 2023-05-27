namespace Functions.Extensions;

public static class BlobClientExtensions
{
    public static async Task<Response<BlobCopyInfo>> CopyToAsync(this BlockBlobClient src, BlockBlobClient dest) =>
        await dest.SyncCopyFromUriAsync(src.Uri);

    public static async Task<bool> MoveToAsync(this BlockBlobClient src, BlockBlobClient dest)
    {
        Response<BlobCopyInfo> response = await src.CopyToAsync(dest);
        bool success = (response.Value.CopyStatus == CopyStatus.Success);
        if (success)
            await src.DeleteAsync();

        return success;
    }
}