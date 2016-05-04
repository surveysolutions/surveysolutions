using System;
using System.Linq;
using System.Threading.Tasks;
using PCLStorage;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.SharedKernels.DataCollection.Implementation.Accessors;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;

namespace WB.Core.BoundedContexts.Interviewer.Implementation.Services
{
    public class InterviewerQuestionnaireAssemblyFileAccessor : IQuestionnaireAssemblyFileAccessor
    {
        private class BackwardCompatibleQuestionnaireAssemblyFileAccessor
        {
            private readonly IFileSystemAccessor fileSystemAccessor;

            private readonly string assemblyStorageDirectory;

            public BackwardCompatibleQuestionnaireAssemblyFileAccessor(string assemblyStorageDirectory, IFileSystemAccessor fileSystemAccessor)
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
        private readonly IAsynchronousFileSystemAccessor asyncFileSystemAccessor;
        private readonly ILogger logger;
        private readonly string pathToStore;
        private readonly BackwardCompatibleQuestionnaireAssemblyFileAccessor backwardCompatibleAccessor;

        public InterviewerQuestionnaireAssemblyFileAccessor(IFileSystemAccessor fileSystemAccessor,
            IAsynchronousFileSystemAccessor asyncFileSystemAccessor, ILogger logger,  string pathToAssembliesDirectory)
        {
            this.fileSystemAccessor = fileSystemAccessor;
            this.asyncFileSystemAccessor = asyncFileSystemAccessor;
            this.logger = logger;
            this.pathToStore = pathToAssembliesDirectory;

            this.backwardCompatibleAccessor = new BackwardCompatibleQuestionnaireAssemblyFileAccessor(this.pathToStore,
                this.fileSystemAccessor);
        }

        public string GetFullPathToAssembly(Guid questionnaireId, long questionnaireVersion)
        {
            string assemblyFileName = this.GetAssemblyFileName(questionnaireId, questionnaireVersion);
            var pathToAssembly = this.fileSystemAccessor.CombinePath(this.pathToStore, assemblyFileName);

            if (!this.fileSystemAccessor.IsFileExists(pathToAssembly))
                return this.backwardCompatibleAccessor.GetFullPathToAssembly(questionnaireId, questionnaireVersion);

            return pathToAssembly;
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
            var assembliesDirectory = await FileSystem.Current.GetFolderFromPathAsync(this.pathToStore);

            string assemblyFileName = this.GetAssemblyFileName(questionnaireIdentity.QuestionnaireId, questionnaireIdentity.Version);

            var assemblyFile = await assembliesDirectory.CreateFileAsync(assemblyFileName, CreationCollisionOption.ReplaceExisting);
            using (var fileHandler = await assemblyFile.OpenAsync(FileAccess.ReadAndWrite))
            {
                await fileHandler.WriteAsync(assembly, 0, assembly.Length);
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

        public async Task RemoveAssemblyAsync(QuestionnaireIdentity questionnaireIdentity)
        {
            string assemblyFileName = this.GetAssemblyFileName(questionnaireIdentity.QuestionnaireId, questionnaireIdentity.Version);
            
            await this.asyncFileSystemAccessor.DeleteFileAsync(this.asyncFileSystemAccessor.CombinePath(this.pathToStore, assemblyFileName));
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
        {
            return this.fileSystemAccessor.IsFileExists(this.GetFullPathToAssembly(questionnaireId, questionnaireVersion));
        }

        private string GetAssemblyFileName(Guid questionnaireId, long questionnaireVersion)
        {
            return $"assembly_{questionnaireId}_v{questionnaireVersion}.dll";
        }
    }
}