using Amazon;
using Amazon.S3;

namespace WB.Services.Export.Services.Processing.Good
{
    public class AmazonS3Settings
    {
        public string BucketName { get; set; }
        public string Region { get; set; } = "us-east-1";
        public string Prefix { get; set; }
        public string Endpoint { get; set; }
        public string Folder { get; set; } = "hq";

        public string BasePath => $"{Folder}/{Prefix}";

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