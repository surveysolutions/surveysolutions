using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Amazon.Runtime;
using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Transfer;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using WB.Core.BoundedContexts.Headquarters;
using WB.Core.BoundedContexts.Headquarters.Storage.AmazonS3;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Tests.Abc;

namespace WB.Tests.Unit.BoundedContexts.Headquarters.Storage
{
    [TestFixture]
    public class S3FileStorageTests
    {
        private AmazonS3ExternalFileStorage storage;
        private AmazonS3Configuration settings;
        private AmazonBucketInfo bucketInfo;
        private Mock<IAmazonS3> client;
        private Mock<ITransferUtility> transferUtility;
        
        [SetUp]
        public void SetUp()
        {
            this.settings = new AmazonS3Configuration(
                Options.Create(new FileStorageConfig
                {
                    AppData = $"s3://test/base"
                }),
                Options.Create(new HeadquartersConfig
                {
                    TenantName = "fiji"
                }), Create.Service.WorkspaceContextAccessor()
            );

            bucketInfo = this.settings.GetAmazonS3BucketInfo();

            this.client = new Mock<IAmazonS3>();
            this.transferUtility = new Mock<ITransferUtility>();

            this.storage = Create.Storage.AmazonS3ExternalFileStorage(settings, 
                client.Object, transferUtility.Object);
        }

        [Test]
        public async Task should_provide_proper_prefix_to_key_when_get_binary()
        {
            client.Setup(c => c.GetObjectAsync(It.IsAny<GetObjectRequest>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new GetObjectResponse()
                {
                    ResponseStream = new MemoryStream()
                });

            await this.storage.GetBinaryAsync("somePath");

            var expectedKey = this.bucketInfo.PathTo("somePath");

            client.Verify(c => c.GetObjectAsync(
                It.Is<GetObjectRequest>(r =>
                    r.BucketName == bucketInfo.BucketName
                    && r.Key == expectedKey), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Test]
        public async Task should_not_throw_if_key_no_found_when_get_binary()
        {
            client.Setup(c => c.GetObjectAsync(It.IsAny<GetObjectRequest>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new AmazonS3Exception("Error", ErrorType.Sender, "NoSuchKey", "", HttpStatusCode.NotFound));

            var binary = await this.storage.GetBinaryAsync("somePath");
            Assert.That(binary, Is.Null);
        }

        [Test]
        public async Task should_provide_proper_prefix_to_key_when_get_list()
        {
            client.Setup(c => c.ListObjectsV2Async(It.IsAny<ListObjectsV2Request>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ListObjectsV2Response
                {
                    S3Objects = new List<S3Object>()
                });

            await this.storage.ListAsync("somePath");

            var expectedKey = this.bucketInfo.PathTo("/somePath");

            client.Verify(c => c.ListObjectsV2Async(
                It.Is<ListObjectsV2Request>(r =>
                    r.BucketName == bucketInfo.BucketName
                    && r.Prefix == expectedKey), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Test]
        public async Task should_return_objects_with_normalized_keys()
        {
            client.Setup(c => c.ListObjectsV2Async(It.IsAny<ListObjectsV2Request>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ListObjectsV2Response
                {
                    S3Objects = new List<S3Object>
                    {
                        new S3Object{Key = this.bucketInfo.PathTo("/one")},
                        new S3Object{Key = this.bucketInfo.PathTo("/two")}
                    }
                });

            var result = await this.storage.ListAsync("");

            Assert.That(result, Has.One.Property(nameof(FileObject.Path)).EqualTo("one"));
            Assert.That(result, Has.One.Property(nameof(FileObject.Path)).EqualTo("two"));
        }

        [Test]
        public void should_provide_proper_prefix_to_key_when_get_direct_link()
        {
            client.Setup(c => c.GetPreSignedURL(It.IsAny<GetPreSignedUrlRequest>()))
                .Returns("link");

            this.storage.GetDirectLink("somePath", TimeSpan.Zero);

            var expectedKey = this.bucketInfo.PathTo("/somePath");

            client.Verify(c => c.GetPreSignedURL(
                It.Is<GetPreSignedUrlRequest>(r =>
                    r.BucketName == bucketInfo.BucketName
                    && r.Key == expectedKey)), Times.Once);
        }

        //[Test]
        //public void should_use_proper_keys_for_upload()
        //{
        //    transferUtility.Setup(tu => tu.Upload(It.IsAny<TransferUtilityUploadRequest>()));

        //    this.storage.Store("somekey", new byte[] { 1, 2, 3, 4, 5 }, String.Empty, null);

        //    transferUtility.Verify(tu => tu.Upload(It.Is<TransferUtilityUploadRequest>(
        //        tr => tr.BucketName == bucketInfo.BucketName && tr.Key == this.bucketInfo.PathTo("/somekey"))), Times.Once);
        //}

        [Test]
        public async Task should_use_proper_key_for_deletion()
        {
            client.Setup(c => c.DeleteObjectAsync(It.IsAny<DeleteObjectRequest>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new DeleteObjectResponse());

            await this.storage.RemoveAsync("somePath");

            client.Verify(c => c.DeleteObjectAsync(bucketInfo.BucketName, this.bucketInfo.PathTo("/somePath"), 
                It.IsAny<CancellationToken>()), Times.Once);
        }

        [Test]
        public void should_properly_generate_pathTo_key_and_normalize_slashes()
        {
            Assert.That(bucketInfo.PathTo("/path"), Is.EqualTo("base/fiji/path"));
            Assert.That(bucketInfo.PathTo("path"), Is.EqualTo("base/fiji/path"));
        }

        [Test]
        public void should_extract_bucket_name_and_base_path_from_hq_appdata_config()
        {
            var bucket = new AmazonS3Configuration(
                Options.Create(new FileStorageConfig
                {
                    AppData = $"s3://another.bucket.name/test/for/folder"
                }),
                Options.Create(new HeadquartersConfig
                {
                    TenantName = "fiji"
                }), Create.Service.WorkspaceContextAccessor()
            ).GetAmazonS3BucketInfo();

            Assert.That(bucket.BucketName, Is.EqualTo("another.bucket.name"));
            Assert.That(bucket.PathPrefix, Is.EqualTo("test/for/folder/fiji/"), "Should add tenant name from hq config");
        }

        [TestCase("s3://some/path", ExpectedResult = StorageProviderType.AmazonS3)]
        [TestCase("~/App_Data", ExpectedResult = StorageProviderType.FileSystem)]
        [TestCase("c:/app_data", ExpectedResult = StorageProviderType.FileSystem)]
        public StorageProviderType fileStorage_Config_should_return_proper_storage_provider_type(string appData)
        {
            var config = new FileStorageConfig
            {
                AppData = appData
            };

            return config.GetStorageProviderType();
        }
    }
}
