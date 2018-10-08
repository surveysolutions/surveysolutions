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
        public string MinioUrl { get; set; }
        public string AccessKey { get; set; }
        public string SecretKey { get; set; }
        public string Region { get; set; } = RegionEndpoint.USEast1.SystemName;

        public AmazonS3Config Config()
        {
            var config = new AmazonS3Config();
            
            config.RegionEndpoint = RegionEndpoint.GetBySystemName(Region);

            // support for local dev env endpoints
            if (!string.IsNullOrWhiteSpace(MinioUrl))
            {
                config.ServiceURL = MinioUrl;
                config.ForcePathStyle = true;
            }

            return config;
        }
    }
}
