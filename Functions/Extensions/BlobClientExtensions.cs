using Azure.Storage;

namespace Functions.Extensions;

public static class BlobClientExtensions
{
    public static async Task<Response<BlobCopyInfo>> CopyToAsync(this BlockBlobClient src, BlockBlobClient dest) =>
        await dest.SyncCopyFromUriAsync(src.SasUri());

    public static async Task<bool> MoveToAsync(this BlockBlobClient src, BlockBlobClient dest)
    {
        Response<BlobCopyInfo> response = await src.CopyToAsync(dest);
        bool success = (response.Value.CopyStatus == CopyStatus.Success);
        if (success)
            await src.DeleteAsync();

        return success;
    }

    private static Uri SasUri(this BlockBlobClient src)
    {
        Azure.Storage.Sas.BlobSasBuilder blobSasBuilder = new Azure.Storage.Sas.BlobSasBuilder()
        {
            BlobContainerName = src.BlobContainerName,
            BlobName = src.Name,
            ExpiresOn = DateTime.UtcNow.Add(Configuration.SasTokenValidityDuration),
        };
        blobSasBuilder.SetPermissions(Azure.Storage.Sas.BlobSasPermissions.Read | Azure.Storage.Sas.BlobSasPermissions.Delete);
        string sasToken = blobSasBuilder.ToSasQueryParameters(new StorageSharedKeyCredential(Configuration.StorageAccountName, Configuration.StorageAccountKey)).ToString();
        return new Uri($"{src.Uri.AbsoluteUri}?{sasToken}");
    }
}