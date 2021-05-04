#nullable enable
using System;
using System.IO;

namespace WB.Core.SharedKernels.DataCollection
{
    public class FileStorageConfig
    {
        public string AppData { get; set; } = "~/App_Data";
        public string TempData { get;  set; } = "~/App_Data";
        public string GlobalTempData { get; set; } = "~/App_Data";

        public StorageProviderType? StorageProviderType { get; set; }
        
        public string? FFmpegExecutablePath { get; set; }

        private StorageProviderType? storageType;

        public StorageProviderType GetStorageProviderType()
        {
            if (storageType != null) return storageType.Value;

            if (StorageProviderType != null) return StorageProviderType.Value;

            storageType = AppData.StartsWith("s3://", StringComparison.OrdinalIgnoreCase) 
                ? DataCollection.StorageProviderType.AmazonS3 
                : DataCollection.StorageProviderType.FileSystem;

            return storageType.Value;
        }
    }

    public enum StorageProviderType
    {
        FileSystem = 0,
        AmazonS3 = 1
    }
}
