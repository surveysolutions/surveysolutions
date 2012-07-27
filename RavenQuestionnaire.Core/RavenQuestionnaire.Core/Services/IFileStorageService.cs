using System;
using System.Collections.Generic;
using System.IO;
using Raven.Abstractions.Data;
using Raven.Json.Linq;
using RavenQuestionnaire.Core.Documents;

namespace RavenQuestionnaire.Core.Services
{
    public interface IFileStorageService
    {
        void StoreFile(FileDescription file);

        FileDescription RetrieveFile(string filename);

        void DeleteFile(string filename);
    }
}
