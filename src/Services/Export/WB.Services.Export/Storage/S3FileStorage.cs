using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Transfer;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using WB.Services.Export.Services.Processing;

namespace WB.Services.Export.Storage
{
    public class S3ArtifactsStorage : IExternalArtifactsStorage
    {
        private readonly IOptions<S3StorageSettings> s3Settings;
        private readonly IAmazonS3 client;
        private readonly ITransferUtility transferUtility;

        private readonly ILogger<S3ArtifactsStorage> log;

        public S3ArtifactsStorage(IOptions<S3StorageSettings> s3Settings,
            IAmazonS3 amazonS3Client,
            ITransferUtility transferUtility,
            ILogger<S3ArtifactsStorage> log)
        {
            this.s3Settings = s3Settings;
            client = amazonS3Client;
            this.transferUtility = transferUtility;
            this.log = log;
        }

        private string StorageBasePath => $"{S3Settings.BasePath}/";
        private string GetKey(string key) => (StorageBasePath + key).Replace('\\', '/');

        public async Task<byte[]?> GetBinaryAsync(string key)
        {
            try
            {
                var getObject = new GetObjectRequest
                {
                    BucketName = S3Settings.BucketName,
                    Key = GetKey(key)
                };

                log.LogTrace("GetBinary: {bucketName}/{key}", S3Settings.BucketName, getObject.Key);
                using var response = await client.GetObjectAsync(getObject);
                await using var ms = new MemoryStream();
                await response.ResponseStream.CopyToAsync(ms);
                return ms.ToArray();
            }
            catch (AmazonS3Exception e) when (e.StatusCode == HttpStatusCode.NotFound)
            {
                log.LogTrace("Cannot get object from S3. [{statusCode}] {key}", e.StatusCode, GetKey(key));
                return null;
            }
            catch (Exception e)
            {
                LogError("Unable to get binary from {key}", e, key);
                throw;
            }
        }

        public async Task<List<FileObject>?> ListAsync(string prefix)
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
                log.LogTrace("Cannot list objects from S3. [{statusCode}] {prefix}", e.StatusCode, GetKey(prefix));
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
            log.LogError(exception, message +
                ". Bucket: {bucketName}. BasePath: {S3BasePath}",
                args.Union(new[] { S3Settings.BucketName, S3Settings.BasePath }).ToArray());
        }

        public bool IsEnabled() => true;

        private S3StorageSettings S3Settings => s3Settings.Value;

        public string GetDirectLink(string key, TimeSpan expiration, string? asFilename = null)
        {
            var protocol = (client.Config.ServiceURL ?? "https://")
                .StartsWith("https://", StringComparison.OrdinalIgnoreCase)
                ? Protocol.HTTPS
                : Protocol.HTTP;

            log.LogDebug("GetDirectLink: {bucketName}/{key}", S3Settings.BucketName, GetKey(key));

            var preSignedUrlRequest = new GetPreSignedUrlRequest
            {
                Protocol = protocol,
                BucketName = S3Settings.BucketName,
                Key = GetKey(key),
                Expires = DateTime.UtcNow.Add(expiration)
            };

            if (!string.IsNullOrWhiteSpace(asFilename))
            {
                asFilename = WebUtility.UrlEncode(asFilename)?.Replace('+', ' ');
                preSignedUrlRequest.ResponseHeaderOverrides.ContentDisposition = $"attachment; filename =\"{asFilename}\"";
            }
            
            var presignedURL =  client.GetPreSignedURL(preSignedUrlRequest);
            
            if(string.IsNullOrEmpty(presignedURL))
                log.LogWarning($"GetDirectLink: Presigned URL was empty for {key}");
            
            return presignedURL;
        }

        public async Task<FileObject> StoreAsync(string key, Stream inputStream, string contentType,
            ExportProgress? progress = null, CancellationToken cancellationToken = default)
        {
            try
            {
                var uploadRequest = new TransferUtilityUploadRequest
                {
                    BucketName = S3Settings.BucketName,
                    Key = GetKey(key),
                    ContentType = contentType,
                    AutoCloseStream = false,
                    AutoResetStreamPosition = false,
                    InputStream = inputStream
                };
                
                log.LogDebug("Storing: {bucketName}/{key} [{contentType}]", uploadRequest.BucketName, uploadRequest.Key, contentType);

                if (progress != null)
                {
                    uploadRequest.UploadProgressEvent += (sender, args) => { progress.Report(args.PercentDone); };
                }

                await transferUtility.UploadAsync(uploadRequest);
                
                progress?.Report(100);
                
                return new FileObject
                {
                    Path = uploadRequest.Key,
                    Size = inputStream.Position,
                    LastModified = DateTime.UtcNow
                };
            }
            catch (Exception e)
            {
                LogError("Unable to store object in S3. Path: {key}", e, key);
                throw;
            }
        }

        public async Task<FileObject?> GetObjectMetadataAsync(string key)
        {
            try
            {
                var requestKey = GetKey(key);
                var response = await client.GetObjectMetadataAsync(S3Settings.BucketName, requestKey);

                if (response.HttpStatusCode == HttpStatusCode.OK)
                    return new FileObject
                    {
                        Path = key,
                        Size = response.ContentLength,
                        LastModified = response.LastModified
                    };
                
                log.LogWarning($"GetObjectMetadata: metadata request for {key} returned status {response.HttpStatusCode}");
            }
            catch (AmazonS3Exception e) when (e.StatusCode == HttpStatusCode.NotFound)
            {
                log.LogTrace("Cannot get object metadata from S3. [{statusCode}] {key}", e.StatusCode, GetKey(key));
                return null;
            }
            catch (Exception e)
            {
                LogError("Unable to get object metadata in S3. Path: {key}", e, key);
                throw;
            }

            return null;
        }


        public async Task<bool> IsExistAsync(string key)
        {
            return (await GetObjectMetadataAsync(key)) != null;
        }

        public async Task<FileObject> StoreAsync(string path, byte[] data, string contentType,
            ExportProgress? progress = null, CancellationToken cancellationToken = default)
        {
            using (var ms = new MemoryStream(data))
            {
                return await StoreAsync(path, ms, contentType, progress, cancellationToken);
            }
        }

        public async Task RemoveAsync(string path)
        {
            try
            {
                var keyPath = GetKey(path);
                this.log.LogTrace("Removing file {bucketName}/{key}", S3Settings.BucketName, keyPath);
                await client.DeleteObjectAsync(S3Settings.BucketName, keyPath);
            }
            catch (AmazonS3Exception e) when (e.StatusCode == HttpStatusCode.NotFound)
            {
                // ignore
            }
            catch (Exception e)
            {
                LogError("Unable to remove object in S3. Path: {path}", e, path);
                throw;
            }
        }
    }
}
