namespace Functions.Core;

public static class Configuration
{
    private const string NAMES_SPACES_MAPPER = "Mapper";
    public static string DefaultBlobType => Environment.GetEnvironmentVariable("DefaultBlobType");
    public static TimeSpan BlobOutdatedThreshold => TimeSpan.TryParse(Environment.GetEnvironmentVariable("BlobOutdatedThreshold"), out TimeSpan span) ? span : TimeSpan.FromMinutes(5);
    public static TimeSpan BatchedBlobsRetryThreshold => TimeSpan.TryParse(Environment.GetEnvironmentVariable("BatchedBlobsRetryThreshold"), out TimeSpan span) ? span : TimeSpan.FromMinutes(5);
    public static List<string> Namespaces => Environment.GetEnvironmentVariable("Namespaces").Split(",").ToList();
    public static string AzureWebJobsFTPStorage => Environment.GetEnvironmentVariable("AzureWebJobsFTPStorage");
    public static long ZipBatchMinSizeMB => long.TryParse(Environment.GetEnvironmentVariable("ZipBatchMinSizeMB"), out long size) ? size : 10;
    public static long ZipBatchMaxSizeMB => long.TryParse(Environment.GetEnvironmentVariable("ZipBatchMaxSizeMB"), out long size) ? size : 20;
    public static int MaxZipsPerExecution => int.TryParse(Environment.GetEnvironmentVariable("MaxZipsPerExecution"), out int size) ? size : 200;
    public static TimeSpan LeaseDuration => TimeSpan.Parse(Environment.GetEnvironmentVariable("LeaseDuration"));
    public static List<DistributionTarget> DistributionTargets => JsonConvert.DeserializeObject<List<DistributionTarget>>(Environment.GetEnvironmentVariable("DistributionTargets"));
    public static string AzureWebJobsZipStorage => Environment.GetEnvironmentVariable("AzureWebJobsZipStorage");
    public static string StorageAccountName => Environment.GetEnvironmentVariable("StorageAccountName");
    public static string StorageAccountKey => Environment.GetEnvironmentVariable("StorageAccountKey");
    public static TimeSpan SasTokenValidityDuration => TimeSpan.Parse(Environment.GetEnvironmentVariable("SasTokenValidityDuration"));

    

    public static string GetNamespaceVariable(string @namespace) => Environment.GetEnvironmentVariable($"{@namespace}{NAMES_SPACES_MAPPER}");
}