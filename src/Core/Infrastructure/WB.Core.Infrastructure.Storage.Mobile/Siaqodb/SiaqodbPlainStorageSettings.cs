using System;
using System.Collections.Generic;

namespace WB.Core.Infrastructure.Storage.Mobile.Siaqodb
{
    public class SiaqodbPlainStorageSettings
    {
        public string StorageFolderPath { get; set; }
        public Dictionary<Type, string> StorageFileNamesByEntityType { get; set; }
    }
}