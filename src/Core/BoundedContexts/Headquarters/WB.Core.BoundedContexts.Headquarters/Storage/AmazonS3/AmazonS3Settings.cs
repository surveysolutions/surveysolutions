using Amazon;
using Amazon.S3;
using Microsoft.Extensions.Options;

namespace WB.Core.BoundedContexts.Headquarters.Storage.AmazonS3
{
    public class AmazonS3Settings
    {
        public string BucketName { get; set; }
        public string Region { get; set; } = "us-east-1";
        public string Endpoint { get; set; }
        public string Folder { get; set; } = "hq";
        public bool IsEnabled { get; set; }
        public string BasePath(string tenantName) => $"{Folder}/{tenantName}";

        public AmazonS3Config Config()
        {
            var config = new AmazonS3Config();
            config.RegionEndpoint = RegionEndpoint.GetBySystemName(Region);

            // support for local dev env endpoints
            if (!string.IsNullOrWhiteSpace(Endpoint))
            {
                config.ServiceURL = Endpoint;
                config.ForcePathStyle = true;
            }

            return config;
        }
    }
}
