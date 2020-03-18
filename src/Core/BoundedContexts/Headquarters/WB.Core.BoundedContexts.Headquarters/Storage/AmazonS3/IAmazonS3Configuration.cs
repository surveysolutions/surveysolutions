namespace WB.Core.BoundedContexts.Headquarters.Storage.AmazonS3
{
    public interface IAmazonS3Configuration
    {
        AmazonBucketInfo GetAmazonS3BucketInfo();
    }
}
