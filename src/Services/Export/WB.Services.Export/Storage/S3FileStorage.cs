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
using Microsoft.Extensions.Options;
using WB.Services.Export.Services.Processing;

namespace WB.Services.Export.Storage
{
    public class S3FileStorage : IExternalFileStorage
    {
        private readonly IOptions<S3StorageSettings> s3Settings;
        private readonly IAmazonS3 client;
        private readonly ITransferUtility transferUtility;

        private readonly ILogger<S3FileStorage> log;

        public S3FileStorage(IOptions<S3StorageSettings> s3Settings,
            IAmazonS3 amazonS3Client,
            ITransferUtility transferUtility,
            ILogger<S3FileStorage> log)
        {
            this.s3Settings = s3Settings;
            client = amazonS3Client;
            this.transferUtility = transferUtility;
            this.log = log;
        }

        private string StorageBasePath => $"{S3Settings.BasePath}/";
        private string GetKey(string key) => (StorageBasePath + key).Replace('\\', '/');

        public async Task<byte[]> GetBinaryAsync(string key)
        {
            try
            {
                var getObject = new GetObjectRequest
                {
                    BucketName = S3Settings.BucketName,
                    Key = GetKey(key)
                };

                log.LogDebug($"GetBinary: {S3Settings.BucketName}/{getObject.Key}");
                using (var response = await client.GetObjectAsync(getObject))
                {
                    using (var ms = new MemoryStream())
                    {
                        response.ResponseStream.CopyTo(ms);
                        return ms.ToArray();
                    }
                }
            }
            catch (AmazonS3Exception e) when (e.StatusCode == HttpStatusCode.NotFound)
            {
                log.LogTrace($"Cannot get object from S3. [{e.StatusCode.ToString()}] {GetKey(key)}");
                return null;
            }
            catch (Exception e)
            {
                LogError($"Unable to get binary from {key}", e);
                throw;
            }
        }

        public async Task<List<FileObject>> ListAsync(string prefix)
        {
            try
            {
                var listObjects = new ListObjectsV2Request
                {
                    BucketName = S3Settings.BucketName,
                    Prefix = GetKey(prefix)
                };

                ListObjectsV2Response response = await client.ListObjectsV2Async(listObjects);

                return response.S3Objects.Select(s3 => new FileObject
                {
                    Path = s3.Key.Substring(StorageBasePath.Length),
                    Size = s3.Size,
                    LastModified = s3.LastModified
                }).ToList();
            }
            catch (AmazonS3Exception e) when (e.StatusCode == HttpStatusCode.NotFound)
            {
                log.LogTrace($"Cannot list objects from S3. [{e.StatusCode.ToString()}] {GetKey(prefix)}");
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
            log.LogError(exception, $"{message}. Bucket: {S3Settings.BucketName}. BasePath: {S3Settings.BasePath} EndPoint: {client.Config.ServiceURL} ");
        }

        public bool IsEnabled() => true;

        private S3StorageSettings S3Settings => s3Settings.Value;

        public string GetDirectLink(string key, TimeSpan expiration)
        {
            var protocol = (client.Config.ServiceURL ?? "https://")
                .StartsWith("https://", StringComparison.OrdinalIgnoreCase)
                ? Protocol.HTTPS
                : Protocol.HTTP;

            log.LogDebug($"GetDirectLink: {S3Settings.BucketName}/{key}");
            
            return client.GetPreSignedURL(new GetPreSignedUrlRequest
            {
                Protocol = protocol,
                BucketName = S3Settings.BucketName,
                Key = GetKey(key),
                Expires = DateTime.UtcNow.Add(expiration)
            });
        }

        public async Task<FileObject> StoreAsync(string key, Stream inputStream, string contentType, IProgress<int> progress = null)
        {
            try
            {
                log.LogDebug($"Store: {S3Settings.BucketName}/{key} [{contentType}]");

                var uploadRequest = new TransferUtilityUploadRequest
                {
                    BucketName = S3Settings.BucketName,
                    Key = GetKey(key),
                    ContentType = contentType,
                    AutoCloseStream = false,
                    AutoResetStreamPosition = false,
                    InputStream = inputStream
                };

                if (progress != null)
                {
                    uploadRequest.UploadProgressEvent += (sender, args) => { progress.Report(args.PercentDone); };
                }

                await transferUtility.UploadAsync(uploadRequest);

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

        public async Task<FileObject> GetObjectMetadataAsync(string key)
        {
            try
            {
                var response = await client.GetObjectMetadataAsync(S3Settings.BucketName, GetKey(key));

                if (response.HttpStatusCode == HttpStatusCode.OK)
                    return new FileObject
                    {
                        Path = key,
                        Size = response.ContentLength,
                        LastModified = response.LastModified
                    };
            }
            catch (AmazonS3Exception e) when (e.StatusCode == HttpStatusCode.NotFound)
            {
                log.LogTrace($"Cannot get object metadata from S3. [{e.StatusCode.ToString()}] {GetKey(key)}");
                return null;
            }
            catch (Exception e)
            {
                LogError($"Unable to remove object in S3. Path: {key}", e);
                throw;
            }

            return null;
        }


        public async Task<bool> IsExistAsync(string key)
        {
            return (await GetObjectMetadataAsync(key)) != null;
        }

        public async Task<FileObject> StoreAsync(string path, byte[] data, string contentType, IProgress<int> progress = null)
        {
            using (var ms = new MemoryStream(data))
            {
                return await StoreAsync(path, ms, contentType, progress);
            }
        }

        public async Task RemoveAsync(string path)
        {
            try
            {
                await client.DeleteObjectAsync(S3Settings.BucketName, GetKey(path));
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
    }
}
