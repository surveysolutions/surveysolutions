﻿using System;
using Amazon;
using Amazon.S3;

namespace WB.Services.Export.Services.Processing
{
    public class S3StorageSettings
    {
        public string BucketName { get; set; } = String.Empty;
        public string Folder { get; set; } = "export";

        public string BasePath => $"{Folder}/{Prefix}";
        public string Prefix { get; set; } = String.Empty;
    }

    public class AmazonS3Settings
    {
        public string? AccessKey { get; set; }
        public string? SecretKey { get; set; }
    }
}
