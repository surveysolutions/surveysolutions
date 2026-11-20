using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.SharedKernels.DataCollection.Implementation.Accessors;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;

namespace WB.UI.Tester.Infrastructure.Internals.Storage
{
    public class TesterQuestionnaireAssemblyAccessor : IQuestionnaireAssemblyAccessor
    {
        private readonly IFileSystemAccessor fileSystemAccessor;

        private readonly string assemblyStorageDirectory;
        private readonly HashSet<Guid> initializedQuestionnaires = new HashSet<Guid>();
        private readonly object sync = new object();

        public TesterQuestionnaireAssemblyAccessor(string assemblyStorageDirectory, IFileSystemAccessor fileSystemAccessor)
        {
            this.fileSystemAccessor = fileSystemAccessor;
            this.assemblyStorageDirectory = assemblyStorageDirectory;
        }
        
        public void StoreAssembly(Guid questionnaireId, long questionnaireVersion, byte[] assembly)
        {
            throw new NotImplementedException();
        }

        public Task StoreAssemblyAsync(QuestionnaireIdentity questionnaireIdentity, byte[] assembly)
        {
            throw new NotImplementedException();
        }

        public Assembly LoadAssembly(Guid questionnaireId, long questionnaireVersion)
        {
            var path = CheckAndGetFullPathToAssemblyOrEmpty(questionnaireId, questionnaireVersion);
            if (string.IsNullOrEmpty(path))
                return null;

            //please don't use LoadFile or Load here, but use LoadFrom
            //dependent assemblies could not be resolved
            return Assembly.LoadFrom(path);
        }

        public void StoreAssembly(Guid questionnaireId, long questionnaireVersion, string assemblyAsBase64String)
        {
            string folderName = this.GetFolderNameForTemplate(questionnaireId);
            string pathToFolder = this.fileSystemAccessor.CombinePath(this.assemblyStorageDirectory, folderName);

            lock (this.sync)
            {
                if (!this.fileSystemAccessor.IsDirectoryExists(pathToFolder))
                {
                    this.fileSystemAccessor.CreateDirectory(pathToFolder);
                }
                else if (!this.initializedQuestionnaires.Contains(questionnaireId))
                {
                    foreach (var file in this.fileSystemAccessor.GetFilesInDirectory(pathToFolder))
                    {
                        this.fileSystemAccessor.DeleteFile(file);
                    }
                }

                var pathToSaveAssembly = this.fileSystemAccessor.CombinePath(pathToFolder, $"{questionnaireVersion}.dll");
                this.fileSystemAccessor.WriteAllBytes(pathToSaveAssembly, Convert.FromBase64String(assemblyAsBase64String));
                this.fileSystemAccessor.MarkFileAsReadonly(pathToSaveAssembly);

                this.initializedQuestionnaires.Add(questionnaireId);
            }
        }

        public void RemoveAssembly(Guid questionnaireId, long questionnaireVersion)
        {

        }

        public void RemoveAssembly(QuestionnaireIdentity questionnaireIdentity)
        {
            throw new NotImplementedException();
        }

        public string GetAssemblyAsBase64String(Guid questionnaireId, long questionnaireVersion)
        {
            byte[] assemblyAsByteArray = this.GetAssemblyAsByteArray(questionnaireId, questionnaireVersion);

            if (assemblyAsByteArray == null)
                return null;

            return Convert.ToBase64String(assemblyAsByteArray);
        }

        public byte[] GetAssemblyAsByteArray(Guid questionnaireId, long questionnaireVersion)
        {
            var assemblyPath = this.CheckAndGetFullPathToAssemblyOrEmpty(questionnaireId, questionnaireVersion);
            if (string.IsNullOrEmpty(assemblyPath) || !this.fileSystemAccessor.IsFileExists(assemblyPath))
                return null;

            return this.fileSystemAccessor.ReadAllBytes(assemblyPath);
        }

        private string GetFolderNameForTemplate(Guid questionnaireId)
        {
            return $"dir-{questionnaireId}";
        }

        public string CheckAndGetFullPathToAssemblyOrEmpty(Guid questionnaireId, long questionnaireVersion)
        {
            var folderName = this.GetFolderNameForTemplate(questionnaireId);
            var assemblySearchPath = this.fileSystemAccessor.CombinePath(this.assemblyStorageDirectory, folderName);
            var path = this.fileSystemAccessor.CombinePath(assemblySearchPath, $"{questionnaireVersion}.dll");
            return this.fileSystemAccessor.IsFileExists(path) ? path : null;
        }

        public bool IsQuestionnaireAssemblyExists(Guid questionnaireId, long questionnaireVersion)
        {
            var assemblyPath = this.CheckAndGetFullPathToAssemblyOrEmpty(questionnaireId, questionnaireVersion);
            return !string.IsNullOrEmpty(assemblyPath);
        }
    }
}
