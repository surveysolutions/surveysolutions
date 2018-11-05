using Amazon;
using Amazon.S3;

namespace WB.Services.Export.Services.Processing
{
    public class S3StorageSettings
    {
        public string BucketName { get; set; }
        public string Folder { get; set; } = "export";

        public string BasePath => $"{Folder}/{Prefix}";
        public string Prefix { get; set; }
    }

    public class AmazonS3Settings
    {
        public string AccessKey { get; set; }
        public string SecretKey { get; set; }
    }
}
