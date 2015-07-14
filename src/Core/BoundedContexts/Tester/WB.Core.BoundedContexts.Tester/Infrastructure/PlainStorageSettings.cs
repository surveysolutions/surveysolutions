using System;
using System.Collections.Generic;

namespace WB.Core.BoundedContexts.QuestionnaireTester.Infrastructure
{
    public class PlainStorageSettings
    {
        public string StorageFolderPath { get; set; }
        public Dictionary<Type, string> StorageFileNamesByEntityType { get; set; }
    }
}