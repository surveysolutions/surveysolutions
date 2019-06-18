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
    public class S3FileStorage : IExternalFileStorage
    {
        private readonly AmazonS3Settings s3Settings;
        private readonly IAmazonS3 client;
        private readonly ITransferUtility transferUtility;
        private readonly string storageBasePath;
        private readonly ILogger log;

        public S3FileStorage(AmazonS3Settings s3Settings, IAmazonS3 amazonS3Client, ITransferUtility transferUtility, ILoggerProvider loggerProvider)
        {
            log = loggerProvider.GetForType(GetType());
            this.s3Settings = s3Settings;
            client = amazonS3Client;
            this.transferUtility = transferUtility;
            storageBasePath = $"{s3Settings.BasePath}/";
        }

        private string GetKey(string key) => storageBasePath + key;

        public byte[] GetBinary(string key)
        {
            try
            {
                var getObject = new GetObjectRequest
                {
                    BucketName = s3Settings.BucketName,
                    Key = GetKey(key)
                };

                using (var response = client.GetObject(getObject))
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
                log.Trace($"Cannot get object from S3. [{e.StatusCode.ToString()}] {GetKey(key)}");
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
                    BucketName = s3Settings.BucketName,
                    Prefix = GetKey(prefix)
                };

                ListObjectsV2Response response = client.ListObjectsV2(listObjects);
                return response.S3Objects.Select(s3 => new FileObject
                {
                    Path = s3.Key.Substring(storageBasePath.Length),
                    Size = s3.Size,
                    LastModified = s3.LastModified
                }).ToList();
            }
            catch (AmazonS3Exception e) when (e.StatusCode == HttpStatusCode.NotFound)
            {
                log.Trace($"Cannot list objects from S3. [{e.StatusCode.ToString()}] {GetKey(prefix)}");
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
                      $"Bucket: {s3Settings.BucketName}. " +
                      $"BasePath: {s3Settings.BasePath} " +
                      $"EndPoint: {s3Settings.Endpoint} ", exception);
        }

        public bool IsEnabled() => true;

        public string GetDirectLink(string key, TimeSpan expiration)
        {
            var protocol = string.IsNullOrWhiteSpace(s3Settings.Endpoint) 
                           || s3Settings.Endpoint.StartsWith("https://", StringComparison.OrdinalIgnoreCase)
                ? Protocol.HTTPS
                : Protocol.HTTP;

            return client.GetPreSignedURL(new GetPreSignedUrlRequest
            {
                Protocol = protocol,
                BucketName = s3Settings.BucketName,
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
                    BucketName = s3Settings.BucketName,
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

        public async Task<FileObject> StoreAsync(string key, Stream inputStream, string contentType, IProgress<int> progress = null)
        {
            try
            {
                var uploadRequest = new TransferUtilityUploadRequest
                {
                    BucketName = s3Settings.BucketName,
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

        public FileObject GetObjectMetadata(string key)
        {
            try
            {
                var response = client.GetObjectMetadata(s3Settings.BucketName, GetKey(key));

                if (response.HttpStatusCode == HttpStatusCode.OK) return new FileObject
                {
                    Path = key,
                    Size = response.ContentLength,
                    LastModified = response.LastModified
                };
            }
            catch (AmazonS3Exception e) when (e.StatusCode == HttpStatusCode.NotFound)
            {
                log.Trace($"Cannot get object metadata from S3. [{e.StatusCode.ToString()}] {GetKey(key)}");
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
                client.DeleteObject(s3Settings.BucketName, GetKey(path));
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
