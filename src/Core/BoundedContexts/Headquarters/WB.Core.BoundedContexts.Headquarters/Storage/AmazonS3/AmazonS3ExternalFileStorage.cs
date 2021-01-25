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
using Microsoft.Extensions.Logging;
using WB.Core.SharedKernels.DataCollection.Repositories;

namespace WB.Core.BoundedContexts.Headquarters.Storage.AmazonS3
{
    public class AmazonS3ExternalFileStorage : IExternalFileStorage
    {
        private readonly IAmazonS3Configuration s3Configuration;
        private readonly IAmazonS3 client;
        private readonly ITransferUtility transferUtility;
        private readonly ILogger<AmazonS3ExternalFileStorage> logger;

        public AmazonS3ExternalFileStorage(
            IAmazonS3Configuration s3Configuration,
            IAmazonS3 amazonS3Client,
            ITransferUtility transferUtility,
            ILogger<AmazonS3ExternalFileStorage> logger)
        {
            this.s3Configuration = s3Configuration;
            client = amazonS3Client;
            this.transferUtility = transferUtility;
            this.logger = logger;
        }

        private AmazonBucketInfo? _bucketInfo;
        private AmazonBucketInfo BucketInfo => _bucketInfo ??= this.s3Configuration.GetAmazonS3BucketInfo();

        public async Task<byte[]?> GetBinaryAsync(string key)
        {
            var getObject = new GetObjectRequest
            {
                BucketName = BucketInfo.BucketName,
                Key = BucketInfo.PathTo(key)
            };

            try
            {
                using var response = await client.GetObjectAsync(getObject).ConfigureAwait(false);
                await using var ms = new MemoryStream();
                await response.ResponseStream.CopyToAsync(ms);
                logger.LogTrace("Got object from S3. [{key}] {request_key}", key, getObject.Key);
                return ms.ToArray();
            }
            catch (AmazonS3Exception e) when (e.StatusCode == HttpStatusCode.NotFound)
            {
                logger.LogTrace("Cannot get object from S3: {statusCode} [{key}] {request_key}", e.StatusCode, key, getObject.Key);
                return null;
            }
            catch (Exception e)
            {
                LogError("Unable to get binary from ]{key}] {request_key}", e, key, getObject.Key);
                throw;
            }
        }

        public async Task<List<FileObject>?> ListAsync(string prefix)
        {
            var listObjects = new ListObjectsV2Request
            {
                BucketName = BucketInfo.BucketName,
                Prefix = BucketInfo.PathTo(prefix)
            };

            try
            {
                ListObjectsV2Response response = await client.ListObjectsV2Async(listObjects).ConfigureAwait(false);
                logger.LogTrace("Got list of objects from S3. Prefix: [{prefix}] {request_prefix}, Count: {count}",
                    prefix, listObjects.Prefix, response.S3Objects.Count);

                return response.S3Objects.Select(s3 => new FileObject
                {
                    Path = s3.Key.Substring(BucketInfo.PathPrefix.Length),
                    Size = s3.Size,
                    LastModified = s3.LastModified
                }).ToList();
            }
            catch (AmazonS3Exception e) when (e.StatusCode == HttpStatusCode.NotFound)
            {
                logger.LogTrace("Cannot list objects from S3. [{statusCode}] {prefix}", e.StatusCode, listObjects.Prefix);
                return null;
            }
            catch (Exception e)
            {
                LogError("Unable to get list of object from S3 by prefix: {prefix}", e, prefix);
                throw;
            }
        }

        private void LogError(string message, Exception exception, params object[] args)
        {
            logger.LogError(exception, message + ". " +
                      "BucketName: {BucketInfo.BucketName}. " +
                      "BasePath: {BucketInfo.PathPrefix} ", new [] { BucketInfo.BucketName, BucketInfo.PathPrefix}
                .Concat(args).ToArray());
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
            var uploadRequest = new TransferUtilityUploadRequest
            {
                BucketName = BucketInfo.BucketName,
                Key = BucketInfo.PathTo(key),
                ContentType = contentType,
                AutoCloseStream = false,
                AutoResetStreamPosition = false,
                InputStream = inputStream
            };

            try
            {
                if (progress != null)
                {
                    uploadRequest.UploadProgressEvent += (sender, args) => { progress.Report(args.PercentDone); };
                }

                transferUtility.Upload(uploadRequest);

                logger.LogTrace("Stored object to S3. [{key}] {request_key}", key, uploadRequest.Key);

                return new FileObject
                {
                    Path = uploadRequest.Key,
                    Size = inputStream.Position,
                    LastModified = DateTime.UtcNow
                };
            }
            catch (Exception e)
            {
                LogError("Unable to store object in S3. [{key}] {request_key}",e , key, uploadRequest.Key);
                throw;
            }
        }

        public async Task<FileObject> StoreAsync(string key,
            Stream inputStream,
            string contentType,
            IProgress<int>? progress = null)
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

            try
            {
                if (progress != null)
                {
                    uploadRequest.UploadProgressEvent += (sender, args) => { progress.Report(args.PercentDone); };
                }

                await transferUtility.UploadAsync(uploadRequest).ConfigureAwait(false);
                logger.LogTrace("Stored object to S3. [{key}] {request_key}", key, uploadRequest.Key);

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
                logger.LogTrace("Deleted object from S3. [{path}] {request_path}", path, BucketInfo.PathTo(path));
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
                    var objects = string.Join(", ", deleteRequest.Objects.Select(kl => kl.Key));
                    logger.LogTrace("Deleted object from S3. [{objects}]", objects);
                }
            }
            catch (AmazonS3Exception e) when (e.StatusCode == HttpStatusCode.NotFound)
            {
                // ignore
                logger.LogInformation("Unable to find object in S3. " +
                                      $"BucketName: {BucketInfo.BucketName}. BasePath: {BucketInfo.PathPrefix} ");
            }
            catch (Exception e)
            {
                LogError($"Unable to remove object in S3. Paths: {paths}", e);
                throw;
            }
        }
    }
}
