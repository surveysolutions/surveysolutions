using System;
using System.Collections.Generic;
using System.IO;
using Raven.Abstractions.Data;
using Raven.Json.Linq;

namespace RavenQuestionnaire.Core.Services
{
    public interface IFileStorageService
    {
        void StoreFile(string filename, Stream bytes);

        byte[] RetrieveFile(string filename);

        void DeleteFile(string filename);
    }
}
