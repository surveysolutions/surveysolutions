using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.SharedKernels.DataCollection.Implementation.Accessors;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;

namespace WB.UI.Shared.Enumerator.Services
{
    public class InterviewerQuestionnaireAssemblyAccessor : IQuestionnaireAssemblyAccessor
    {
        private class BackwardCompatibleQuestionnaireAssemblyFileAccessor
        {
            private readonly IFileSystemAccessor fileSystemAccessor;

            private readonly string assemblyStorageDirectory;

            public BackwardCompatibleQuestionnaireAssemblyFileAccessor(string assemblyStorageDirectory,
                IFileSystemAccessor fileSystemAccessor)
            {
                this.fileSystemAccessor = fileSystemAccessor;
                this.assemblyStorageDirectory = assemblyStorageDirectory;
            }

            public string GetFullPathToAssembly(Guid questionnaireId, long questionnaireVersion)
            {
                return this.GetFullPathToAssembly(questionnaireId);
            }

            public byte[] GetAssemblyAsByteArray(Guid questionnaireId, long questionnaireVersion)
            {
                var assemblyPath = this.GetFullPathToAssembly(questionnaireId);
                if (!this.fileSystemAccessor.IsFileExists(assemblyPath))
                    return null;

                return this.fileSystemAccessor.ReadAllBytes(this.GetFullPathToAssembly(questionnaireId));
            }

            private string GetFolderNameForTemplate(Guid questionnaireId)
            {
                return $"dir-{questionnaireId}";
            }

            private string GetFullPathToAssembly(Guid questionnaireId)
            {
                var folderName = this.GetFolderNameForTemplate(questionnaireId);
                var assemblySearchPath = this.fileSystemAccessor.CombinePath(this.assemblyStorageDirectory, folderName);

                if (!this.fileSystemAccessor.IsDirectoryExists(assemblySearchPath))
                    return null;

                var filesInDirectory = this.fileSystemAccessor.GetFilesInDirectory(assemblySearchPath);

                return filesInDirectory.OrderByDescending(this.fileSystemAccessor.GetCreationTime).FirstOrDefault();
            }
        }

        private readonly IFileSystemAccessor fileSystemAccessor;
        private readonly ILogger logger;
        private readonly string pathToStore;
        private readonly BackwardCompatibleQuestionnaireAssemblyFileAccessor backwardCompatibleAccessor;

        public InterviewerQuestionnaireAssemblyAccessor(IFileSystemAccessor fileSystemAccessor,
            ILogger logger, 
            string pathToAssembliesDirectory)
        {
            this.fileSystemAccessor = fileSystemAccessor;
            this.logger = logger;
            this.pathToStore = pathToAssembliesDirectory;

            this.backwardCompatibleAccessor = new BackwardCompatibleQuestionnaireAssemblyFileAccessor(this.pathToStore,
                this.fileSystemAccessor);
        }

        private string CheckAndGetFullPathToAssemblyOrEmpty(Guid questionnaireId, long questionnaireVersion)
        {
            string assemblyFileName = this.GetAssemblyFileName(questionnaireId, questionnaireVersion);
            var pathToAssembly = this.fileSystemAccessor.CombinePath(this.pathToStore, assemblyFileName);

            if (this.fileSystemAccessor.IsFileExists(pathToAssembly))
                return pathToAssembly;

            var oldPath = this.backwardCompatibleAccessor.GetFullPathToAssembly(questionnaireId, questionnaireVersion);

            return this.fileSystemAccessor.IsFileExists(oldPath) ? oldPath : string.Empty;
        }

        public Assembly LoadAssembly(Guid questionnaireId, long questionnaireVersion)
        {
            var path = this.CheckAndGetFullPathToAssemblyOrEmpty(questionnaireId, questionnaireVersion);
            if (string.IsNullOrEmpty(path))
                return null;

            //please don't use LoadFile or Load here, but use LoadFrom
            //dependent assemblies could not be resolved
            return Assembly.LoadFrom(path);
        }

        public void StoreAssembly(Guid questionnaireId, long questionnaireVersion, string assemblyAsBase64)
        {
            this.StoreAssembly(questionnaireId, questionnaireVersion, Convert.FromBase64String(assemblyAsBase64));
        }

        public void StoreAssembly(Guid questionnaireId, long questionnaireVersion, byte[] assembly)
        {
            string assemblyFileName = this.GetAssemblyFileName(questionnaireId, questionnaireVersion);
            var pathToSaveAssembly = this.fileSystemAccessor.CombinePath(this.pathToStore, assemblyFileName);

            if (assembly.Length == 0)
                throw new Exception(
                    $"Assembly file is empty. Cannot be saved. Questionnaire: {questionnaireId}, version: {questionnaireVersion}");

            this.fileSystemAccessor.WriteAllBytes(pathToSaveAssembly, assembly);

            this.fileSystemAccessor.MarkFileAsReadonly(pathToSaveAssembly);
        }

        public async Task StoreAssemblyAsync(QuestionnaireIdentity questionnaireIdentity, byte[] assembly)
        {
            string assemblyFileName = this.GetAssemblyFileName(questionnaireIdentity.QuestionnaireId, questionnaireIdentity.Version);
            var path = this.fileSystemAccessor.CombinePath(this.pathToStore, assemblyFileName);

            if (this.fileSystemAccessor.IsFileExists(path))
            {
                this.fileSystemAccessor.DeleteFile(path);
            }

            using (var assemblyFile = this.fileSystemAccessor.OpenOrCreateFile(path, false))
            {
                await assemblyFile.WriteAsync(assembly, 0, assembly.Length);
            }
        }

        public void RemoveAssembly(Guid questionnaireId, long questionnaireVersion)
        {
            string assemblyFileName = this.GetAssemblyFileName(questionnaireId, questionnaireVersion);
            var pathToSaveAssembly = this.fileSystemAccessor.CombinePath(this.pathToStore, assemblyFileName);

            this.logger.Info(
                $"Trying to delete assembly for questionnaire {questionnaireId} version {questionnaireVersion}");

            //loaded assembly could be locked
            try
            {
                this.fileSystemAccessor.DeleteFile(pathToSaveAssembly);
            }
            catch (Exception e)
            {
                this.logger.Error(
                    $"Error on assembly deletion for questionnaire {questionnaireId} version {questionnaireVersion}");
                this.logger.Error(e.Message, e);
            }
        }

        public void RemoveAssembly(QuestionnaireIdentity questionnaireIdentity)
        {
            string assemblyFileName = this.GetAssemblyFileName(questionnaireIdentity.QuestionnaireId, questionnaireIdentity.Version);
            
            this.fileSystemAccessor.DeleteFile(this.fileSystemAccessor.CombinePath(this.pathToStore, assemblyFileName));
        }

        public string GetAssemblyAsBase64String(Guid questionnaireId, long questionnaireVersion)
        {
            byte[] assemblyAsByteArray = this.GetAssemblyAsByteArray(questionnaireId, questionnaireVersion);

            return assemblyAsByteArray == null ? null : Convert.ToBase64String(assemblyAsByteArray);
        }

        public byte[] GetAssemblyAsByteArray(Guid questionnaireId, long questionnaireVersion)
        {
            string assemblyFileName = this.GetAssemblyFileName(questionnaireId, questionnaireVersion);
            string pathToAssembly = this.fileSystemAccessor.CombinePath(this.pathToStore, assemblyFileName);

            if (!this.fileSystemAccessor.IsFileExists(pathToAssembly))
                return this.backwardCompatibleAccessor.GetAssemblyAsByteArray(questionnaireId, questionnaireVersion);

            return this.fileSystemAccessor.ReadAllBytes(pathToAssembly);
        }

        public bool IsQuestionnaireAssemblyExists(Guid questionnaireId, long questionnaireVersion)
            => !string.IsNullOrEmpty(this.CheckAndGetFullPathToAssemblyOrEmpty(questionnaireId, questionnaireVersion));

        private string GetAssemblyFileName(Guid questionnaireId, long questionnaireVersion)
            => $"assembly_{questionnaireId}_v{questionnaireVersion}.dll";
    }
}
