using System;

namespace WB.Services.Export.Services.Processing
{
    public class S3StorageSettings
    {
        public string BucketName { get; set; } = String.Empty;
        public string Folder { get; set; } = "export";

        public string BasePath => $"{Folder}/{Prefix}";
        public string Prefix { get; set; } = "export";
        public string Uri { get; set; } = string.Empty;
    }
}
