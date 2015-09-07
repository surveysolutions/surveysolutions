using System;
using System.IO;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.SharedKernels.DataCollection.Implementation.Accessors;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;

namespace WB.Infrastructure.Shared.Enumerator.Internals.FileSystem
{
    internal class QuestionnaireAssemblyFileAccessor : IQuestionnaireAssemblyFileAccessor
    {
        private readonly IFileSystemAccessor fileSystemAccessor;
        
        private readonly string assemblyStorageDirectory;

        public QuestionnaireAssemblyFileAccessor(string assemblyStorageDirectory, IFileSystemAccessor fileSystemAccessor)
        {
            this.fileSystemAccessor = fileSystemAccessor;
            this.assemblyStorageDirectory = assemblyStorageDirectory;
        }

        public void StoreAssembly(Guid questionnaireId, long questionnaireVersion, string assemblyAsBase64String)
        {
            this.StoreAssembly(questionnaireId, questionnaireVersion, Convert.FromBase64String(assemblyAsBase64String));
        }

        public void StoreAssembly(Guid questionnaireId, long questionnaireVersion, byte[] assembly)
        {
            var questionnaireAssemblyFullPath = this.GetFullPathToAssembly(questionnaireId, questionnaireVersion);

            this.fileSystemAccessor.WriteAllBytes(questionnaireAssemblyFullPath, assembly);
            this.fileSystemAccessor.MarkFileAsReadonly(questionnaireAssemblyFullPath);
        }

        public void RemoveAssembly(Guid questionnaireId, long questionnaireVersion)
        {
            
        }

        public string GetAssemblyAsBase64String(Guid questionnaireId, long questionnaireVersion)
        {
            throw new NotImplementedException();
        }

        public byte[] GetAssemblyAsByteArray(Guid questionnaireId, long questionnaireVersion)
        {
            throw new NotImplementedException();
        }

        public Stream GetAssemblyAsStream(Guid questionnaireId, long questionnaireVersion)
        {
            throw new NotImplementedException();
        }

        public string GetFullPathToAssembly(Guid questionnaireId, long questionnaireVersion)
        {
            return this.fileSystemAccessor.CombinePath(this.assemblyStorageDirectory, string.Format("{0}.dll", new QuestionnaireIdentity(questionnaireId, questionnaireVersion)));
        }

        public bool IsQuestionnaireAssemblyExists(Guid questionnaireId, long questionnaireVersion)
        {
            return this.fileSystemAccessor.IsFileExists(this.GetFullPathToAssembly(questionnaireId, questionnaireVersion));
        }
    }
}
