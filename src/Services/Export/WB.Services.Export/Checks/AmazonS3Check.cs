using System.Threading.Tasks;
using Amazon;
using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.Extensions.Options;
using WB.Services.Export.Services.Processing;
using WB.Services.Infrastructure.Health;

namespace WB.Services.Export.Checks
{
    public class AmazonS3Check : IHealthCheck
    {
        private readonly IAmazonS3 amazonS3;
        private readonly S3StorageSettings amazonS3Settings;

        public AmazonS3Check(IAmazonS3 amazonS3, IOptions<S3StorageSettings> amazonS3Settings)
        {
            this.amazonS3 = amazonS3;
            this.amazonS3Settings = amazonS3Settings.Value;
        }

        public async Task<bool> CheckAsync()
        {
            await amazonS3.ListObjectsV2Async(new ListObjectsV2Request
            {
                BucketName = amazonS3Settings.BucketName,
                MaxKeys = 1
            });

            return true;
        }

        public string Name => "Amazon S3 Configuration";
    }
}
