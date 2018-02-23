using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using Amazon.S3;
using Amazon.S3.Model;

namespace WB.Core.BoundedContexts.Headquarters.Storage.AmazonS3
{
    public class S3FileStorage : IFileStorage
    {
        private readonly AmazonS3Settings s3Settings;
        private readonly IAmazonS3 client;
        private readonly string storageBasePath;
        
        public S3FileStorage(AmazonS3Settings s3Settings, IAmazonS3 amazonS3Client)
        {
            this.s3Settings = s3Settings;
            this.client = amazonS3Client;
            this.storageBasePath = $"{s3Settings.Folder}/{s3Settings.Prefix}/";
        }
        
        public byte[] GetBinary(string key)
        {
            var getObject = new GetObjectRequest
            {
                BucketName = this.s3Settings.BucketName,
                Key = storageBasePath + key
            };

            using (var response = client.GetObject(getObject))
            {
                if (response.HttpStatusCode == HttpStatusCode.OK)
                {
                    using (var ms = new MemoryStream())
                    {
                        response.ResponseStream.CopyTo(ms);
                        return ms.ToArray();
                    }
                }

                return null;
            }
        }

        public List<FileObject> List(string prefix)
        {
            var listObjects = new ListObjectsV2Request()
            {
                BucketName = this.s3Settings.BucketName,
                Prefix = storageBasePath + prefix
            };

            var response = client.ListObjectsV2(listObjects);
            return response.S3Objects.Select(s3 => new FileObject
            {
                Path = s3.Key.Substring(storageBasePath.Length),
                Size = s3.Size
            }).ToList();
        }

        public FileObject Store(string path, byte[] data, string contentType)
        {
            using (var ms = new MemoryStream(data))
            {
                var request = new PutObjectRequest
                {
                    BucketName = this.s3Settings.BucketName,
                    Key = storageBasePath + path,
                    InputStream = ms,
                    ContentType = contentType
                };

                client.PutObject(request);
            }

            return new FileObject
            {
                Path = path,
                Size = data.LongLength
            };
        }

        public void Remove(string path)
        {
            client.DeleteObject(s3Settings.BucketName, storageBasePath + path);
        }
    }
}