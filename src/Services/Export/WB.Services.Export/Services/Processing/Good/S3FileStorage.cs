using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Transfer;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace WB.Services.Export.Services.Processing
{
    public class S3FileStorage : IExternalFileStorage
    {
        private readonly IOptions<AmazonS3Settings> s3Settings;
        private readonly IAmazonS3 client;
        private readonly ITransferUtility transferUtility;
        
        private readonly ILogger<S3FileStorage> log;

        public S3FileStorage(IOptions<AmazonS3Settings> s3Settings, 
            IAmazonS3 amazonS3Client, 
            ITransferUtility transferUtility, 
            ILogger<S3FileStorage> log)
        {
            this.s3Settings = s3Settings;
            client = amazonS3Client;
            this.transferUtility = transferUtility;
        }

        private string StorageBasePath => $"{S3Settings.BasePath}/";
        private string GetKey(string key) => StorageBasePath + key;

        public byte[] GetBinary(string key)
        {
            try
            {
                var getObject = new GetObjectRequest
                {
                    BucketName = S3Settings.BucketName,
                    Key = GetKey(key)
                };

                using (var response = client.GetObjectAsync(getObject).Result)
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

        public List<FileObject> List(string prefix)
        {
            try
            {
                var listObjects = new ListObjectsV2Request
                {
                    BucketName = S3Settings.BucketName,
                    Prefix = GetKey(prefix)
                };

                ListObjectsV2Response response = client.ListObjectsV2Async(listObjects).Result;
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
            log.LogError($"{message}. " +
                         $"Bucket: {S3Settings.BucketName}. " +
                         $"BasePath: {S3Settings.BasePath} " +
                         $"EndPoint: {S3Settings.Endpoint} ", exception);
        }

        public bool IsEnabled() => true;

        private AmazonS3Settings S3Settings => s3Settings.Value;

        public string GetDirectLink(string key, TimeSpan expiration)
        {
            var protocol = string.IsNullOrWhiteSpace(S3Settings.Endpoint) || S3Settings.Endpoint.StartsWith("https://", StringComparison.OrdinalIgnoreCase)
                ? Protocol.HTTPS
                : Protocol.HTTP;

            return client.GetPreSignedURL(new GetPreSignedUrlRequest
            {
                Protocol = protocol,
                BucketName = S3Settings.BucketName,
                Key = GetKey(key),
                Expires = DateTime.UtcNow.Add(expiration)
            });
        }

        public FileObject Store(string key, Stream inputStream, string contentType, IProgress<int> progress = null)
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

        public FileObject GetObjectMetadata(string key)
        {
            try
            {
                var response = client.GetObjectMetadataAsync(S3Settings.BucketName, GetKey(key)).Result;

                if (response.HttpStatusCode == HttpStatusCode.OK) return new FileObject
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


        public bool IsExist(string key)
        {
            return GetObjectMetadata(key) != null;
        }

        public FileObject Store(string path, byte[] data, string contentType, IProgress<int> progress = null)
        {
            using (var ms = new MemoryStream(data))
            {
                return Store(path, ms, contentType, progress);
            }
        }

        public void Remove(string path)
        {
            try
            {
                client.DeleteObjectAsync(S3Settings.BucketName, GetKey(path)).Wait();
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