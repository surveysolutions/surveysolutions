using System;
using Raven.Abstractions.Data;

namespace RavenQuestionnaire.Core.Services
{
    public interface IFileStorageService
    {
        void StoreFile(string filename, Byte[] bytes);

        byte[] RetrieveFile(string filename);

        void DeleteFile(string filename);
    }
}
