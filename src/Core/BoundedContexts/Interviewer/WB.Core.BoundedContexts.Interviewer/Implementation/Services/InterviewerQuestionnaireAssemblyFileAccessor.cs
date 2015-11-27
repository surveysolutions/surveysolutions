using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Practices.ServiceLocation;
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

            public string GetAssemblyAsBase64String(Guid questionnaireId, long questionnaireVersion)
            {
                byte[] assemblyAsByteArray = this.GetAssemblyAsByteArray(questionnaireId, questionnaireVersion);

                if (assemblyAsByteArray == null)
                    return null;

                return Convert.ToBase64String(assemblyAsByteArray);
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
                return String.Format("dir-{0}", questionnaireId);
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
        private readonly string pathToStore;
        private readonly BackwardCompatibleQuestionnaireAssemblyFileAccessor backwardCompatibleAccessor;

        private static ILogger Logger
        {
            get { return ServiceLocator.Current.GetInstance<ILogger>(); }
        }

        public InterviewerQuestionnaireAssemblyFileAccessor(IFileSystemAccessor fileSystemAccessor, string folderPath, string assemblyDirectoryName)
        {
            this.fileSystemAccessor = fileSystemAccessor;
            this.pathToStore = fileSystemAccessor.CombinePath(folderPath, assemblyDirectoryName);

            if (!fileSystemAccessor.IsDirectoryExists(this.pathToStore))
                fileSystemAccessor.CreateDirectory(this.pathToStore);

            this.backwardCompatibleAccessor = new BackwardCompatibleQuestionnaireAssemblyFileAccessor(this.pathToStore, this.fileSystemAccessor);
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
                throw new Exception(string.Format("Assembly file is empty. Cannot be saved. Questionnaire: {0}, version: {1}", questionnaireId, questionnaireVersion));

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

            Logger.Info(string.Format("Trying to delete assembly for questionnaire {0} version {1}", questionnaireId, questionnaireVersion));

            //loaded assembly could be locked
            try
            {
                this.fileSystemAccessor.DeleteFile(pathToSaveAssembly);
            }
            catch (Exception e)
            {
                Logger.Error(string.Format("Error on assembly deletion for questionnaire {0} version {1}", questionnaireId, questionnaireVersion));
                Logger.Error(e.Message, e);
            }
        }

        public async Task RemoveAssemblyAsync(QuestionnaireIdentity questionnaireIdentity)
        {
            string assemblyFileName = this.GetAssemblyFileName(questionnaireIdentity.QuestionnaireId, questionnaireIdentity.Version);

            var assemblyFile = await FileSystem.Current.GetFileFromPathAsync(PortablePath.Combine(this.pathToStore, assemblyFileName));

            await assemblyFile.DeleteAsync();
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
            return String.Format("assembly_{0}_v{1}.dll", questionnaireId, questionnaireVersion);
        }
    }
}