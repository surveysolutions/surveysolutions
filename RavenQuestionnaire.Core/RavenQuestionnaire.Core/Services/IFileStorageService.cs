using System;
using Raven.Abstractions.Data;

namespace RavenQuestionnaire.Core.Services
{
    public interface IFileStorageService
    {
        void StoreFile(string filename, Byte[] bytes);

        Attachment RetrieveFile(string filename);
    }
}
