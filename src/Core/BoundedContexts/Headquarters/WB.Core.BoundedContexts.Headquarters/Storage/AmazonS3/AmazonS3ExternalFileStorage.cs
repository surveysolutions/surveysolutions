#nullable enable
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Transfer;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.SharedKernels.DataCollection.Repositories;

namespace WB.Core.BoundedContexts.Headquarters.Storage.AmazonS3
{
    public class AmazonS3ExternalFileStorage : IExternalFileStorage
    {
        private readonly IAmazonS3Configuration s3Configuration;
        private readonly IAmazonS3 client;
        private readonly ITransferUtility transferUtility;
        private readonly ILogger log;

        public AmazonS3ExternalFileStorage(
            IAmazonS3Configuration s3Configuration,
            IAmazonS3 amazonS3Client,
            ITransferUtility transferUtility,
            ILoggerProvider loggerProvider)
        {
            log = loggerProvider.GetForType(GetType());
            this.s3Configuration = s3Configuration;
            client = amazonS3Client;
            this.transferUtility = transferUtility;
        }

        private AmazonBucketInfo? _bucketInfo;
        private AmazonBucketInfo BucketInfo => _bucketInfo ??= this.s3Configuration.GetAmazonS3BucketInfo();

        public async Task<byte[]?> GetBinaryAsync(string key)
        {
            try
            {
                var getObject = new GetObjectRequest
                {
                    BucketName = BucketInfo.BucketName,
                    Key = BucketInfo.PathTo(key)
                };

                using var response = await client.GetObjectAsync(getObject).ConfigureAwait(false);
                await using var ms = new MemoryStream();
                await response.ResponseStream.CopyToAsync(ms);
                return ms.ToArray();
            }
            catch (AmazonS3Exception e) when (e.StatusCode == HttpStatusCode.NotFound)
            {
                log.Trace($"Cannot get object from S3. [{e.StatusCode.ToString()}] {BucketInfo.PathTo(key)}");
                return null;
            }
            catch (Exception e)
            {
                LogError($"Unable to get binary from {key}", e);
                throw;
            }
        }

        public async Task<List<FileObject>?> ListAsync(string prefix)
        {
            try
            {
                var listObjects = new ListObjectsV2Request
                {
                    BucketName = BucketInfo.BucketName,
                    Prefix = BucketInfo.PathTo(prefix)
                };

                ListObjectsV2Response response = await client.ListObjectsV2Async(listObjects).ConfigureAwait(false);
                return response.S3Objects.Select(s3 => new FileObject
                {
                    Path = s3.Key.Substring(BucketInfo.PathPrefix.Length),
                    Size = s3.Size,
                    LastModified = s3.LastModified
                }).ToList();
            }
            catch (AmazonS3Exception e) when (e.StatusCode == HttpStatusCode.NotFound)
            {
                log.Trace($"Cannot list objects from S3. [{e.StatusCode.ToString()}] {BucketInfo.PathTo(prefix)}");
                return null;
            }
            catch (Exception e)
            {
                LogError($"Unable to get list of object from S3 by prefix: {prefix}", e);
                throw;
            }
        }

        private void LogError(string message, Exception exception)
        {
            log.Error($"{message}. " +
                      $"BucketName: {BucketInfo.BucketName}. " +
                      $"BasePath: {BucketInfo.PathPrefix} ", exception);
        }

        public bool IsEnabled() => true;

        public string GetDirectLink(string key, TimeSpan expiration)
        {

            return client.GetPreSignedURL(new GetPreSignedUrlRequest
            {
                Protocol = Protocol.HTTPS,
                BucketName = BucketInfo.BucketName,
                Key = BucketInfo.PathTo(key),
                Expires = DateTime.UtcNow.Add(expiration)
            });
        }

        public FileObject Store(string key, Stream inputStream, string contentType, IProgress<int>? progress = null)
        {
            try
            {
                var uploadRequest = new TransferUtilityUploadRequest
                {
                    BucketName = BucketInfo.BucketName,
                    Key = BucketInfo.PathTo(key),
                    ContentType = contentType,
                    AutoCloseStream = false,
                    AutoResetStreamPosition = false,
                    InputStream = inputStream
                };

                if (progress != null)
                {
                    uploadRequest.UploadProgressEvent += (sender, args) => { progress.Report(args.PercentDone); };
                }

                transferUtility.Upload(uploadRequest);

                return new FileObject
                {
                    Path = uploadRequest.Key,
                    Size = inputStream.Position,
                    LastModified = DateTime.UtcNow
                };
            }
            catch (Exception e)
            {
                LogError($"Unable to store object in S3. Path: {key}", e);
                throw;
            }
        }

        public async Task<FileObject> StoreAsync(string key,
            Stream inputStream,
            string contentType,
            IProgress<int>? progress = null)
        {
            try
            {
                var uploadRequest = new TransferUtilityUploadRequest
                {
                    BucketName = BucketInfo.BucketName,
                    Key = BucketInfo.PathTo(key),
                    ContentType = contentType,
                    AutoCloseStream = false,
                    AutoResetStreamPosition = false,
                    InputStream = inputStream
                };

                if (progress != null)
                {
                    uploadRequest.UploadProgressEvent += (sender, args) => { progress.Report(args.PercentDone); };
                }

                await transferUtility.UploadAsync(uploadRequest).ConfigureAwait(false);

                return new FileObject
                {
                    Path = uploadRequest.Key,
                    Size = inputStream.Position,
                    LastModified = DateTime.UtcNow
                };
            }
            catch (Exception e)
            {
                LogError($"Unable to store object in S3. Path: {key}", e);
                throw;
            }
        }

        public FileObject Store(string path, byte[] data, string contentType, IProgress<int>? progress = null)
        {
            using var ms = new MemoryStream(data);
            return Store(path, ms, contentType, progress);
        }

        public async Task RemoveAsync(string path)
        {
            try
            {
                await client.DeleteObjectAsync(BucketInfo.BucketName, BucketInfo.PathTo(path)).ConfigureAwait(false);
            }
            catch (AmazonS3Exception e) when (e.StatusCode == HttpStatusCode.NotFound)
            {
                // ignore
            }
            catch (Exception e)
            {
                LogError($"Unable to remove object in S3. Path: {path}", e);
                throw;
            }
        }

        public async Task RemoveAsync(IEnumerable<string> paths)
        {
            try
            {
                var deleteRequest = new DeleteObjectsRequest();
                deleteRequest.BucketName = BucketInfo.BucketName;
                deleteRequest.Quiet = true; 

                foreach (var path in paths)
                    deleteRequest.AddKey(BucketInfo.PathTo(path));

                if (deleteRequest.Objects.Any())
                {
                    await client.DeleteObjectsAsync(deleteRequest).ConfigureAwait(false);
                }
            }
            catch (AmazonS3Exception e) when (e.StatusCode == HttpStatusCode.NotFound)
            {
                // ignore
                log.Info($"Unable to find object in S3. BucketName: {BucketInfo.BucketName}. BasePath: {BucketInfo.PathPrefix} ", e);
            }
            catch (Exception e)
            {
                LogError($"Unable to remove object in S3. Paths: {paths}", e);
                throw;
            }
        }
    }
}
