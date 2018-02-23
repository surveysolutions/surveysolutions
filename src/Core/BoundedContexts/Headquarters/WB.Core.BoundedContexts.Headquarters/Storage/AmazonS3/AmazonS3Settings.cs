using Amazon;
using Amazon.S3;

namespace WB.Core.BoundedContexts.Headquarters.Storage.AmazonS3
{
    public class AmazonS3Settings
    {
        public bool IsEnabled { get; set; }
        public string BucketName { get; set; }
        public string Region { get; set; } = "us-east-1";
        public string Prefix { get; set; }
        public string Endpoint { get; set; }
        public string Folder { get; set; } = "hq";

        public AmazonS3Config Config() => new AmazonS3Config
        {
            RegionEndpoint = RegionEndpoint.GetBySystemName(Region),
            ServiceURL = Endpoint,
            ForcePathStyle = true
        };
    }
}