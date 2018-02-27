using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Transfer;
using WB.Core.GenericSubdomains.Portable.Services;

namespace WB.Core.BoundedContexts.Headquarters.Storage.AmazonS3
{
    public class S3FileStorage : IFileStorage
    {
        private const string AmazonExceptionCodeNoSuchKey = "NoSuchKey";
        private readonly AmazonS3Settings s3Settings;
        private readonly IAmazonS3 client;
        private readonly string storageBasePath;
        private readonly ILogger log;

        public S3FileStorage(AmazonS3Settings s3Settings, IAmazonS3 amazonS3Client, ILoggerProvider loggerProvider)
        {
            log = loggerProvider.GetForType(GetType());
            this.s3Settings = s3Settings;
            client = amazonS3Client;
            storageBasePath = $"{s3Settings.BasePath}/files/";
        }

        public byte[] GetBinary(string key)
        {
            try
            {
                var getObject = new GetObjectRequest
                {
                    BucketName = s3Settings.BucketName,
                    Key = storageBasePath + key
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
            catch (AmazonS3Exception e) when (e.ErrorCode == AmazonExceptionCodeNoSuchKey)
            {
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
                    Prefix = storageBasePath + prefix
                };

                var response = client.ListObjectsV2(listObjects);
                return response.S3Objects.Select(s3 => new FileObject
                {
                    Path = s3.Key.Substring(storageBasePath.Length),
                    Size = s3.Size
                }).ToList();
            }
            catch (AmazonS3Exception e) when (e.ErrorCode == AmazonExceptionCodeNoSuchKey)
            {
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

        public string GetDirectLink(string path, TimeSpan expiration)
        {
            return client.GetPreSignedURL(new GetPreSignedUrlRequest
            {
                BucketName = s3Settings.BucketName,
                Key = path,
                Expires = DateTime.UtcNow.Add(expiration)
            });
        }

        public FileObject Store(string path, Stream inputStream, string contentType)
        {
            try
            {
                var tu = new TransferUtility(client);
                tu.Upload(inputStream, s3Settings.BucketName, path);

                return new FileObject
                {
                    Path = path,
                    Size = inputStream.Position
                };
            }
            catch (Exception e)
            {
                LogError($"Unable to store object in S3. Path: {path}", e);
                throw;
            }
        }

        public FileObject Store(string path, byte[] data, string contentType)
        {
            using (var ms = new MemoryStream(data))
            {
                return Store(path, ms, contentType);
            }
        }

        public void Remove(string path)
        {
            try
            {
                client.DeleteObject(s3Settings.BucketName, storageBasePath + path);
            }
            catch (AmazonS3Exception e) when (e.ErrorCode == AmazonExceptionCodeNoSuchKey)
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