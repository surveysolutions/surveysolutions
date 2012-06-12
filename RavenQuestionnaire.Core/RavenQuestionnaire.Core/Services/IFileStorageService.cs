using System;
using System.Collections.Generic;
using Raven.Abstractions.Data;
using Raven.Json.Linq;

namespace RavenQuestionnaire.Core.Services
{
    public interface IFileStorageService
    {
        void StoreFile(string filename, Byte[] bytes);

        byte[] RetrieveFile(string filename);

        void DeleteFile(string filename);
    }
}
