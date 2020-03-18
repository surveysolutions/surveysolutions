namespace WB.Core.BoundedContexts.Headquarters.Storage.AmazonS3
{
    public class AmazonBucketInfo
    {
        public AmazonBucketInfo(string bucketName, string pathPrefix)
        {
            BucketName = bucketName;
            PathPrefix = pathPrefix;
        }

        public string BucketName { get; }
        public string PathPrefix { get; }

        public string PathTo(string key) => PathPrefix + key.TrimStart('/');
    }
}
