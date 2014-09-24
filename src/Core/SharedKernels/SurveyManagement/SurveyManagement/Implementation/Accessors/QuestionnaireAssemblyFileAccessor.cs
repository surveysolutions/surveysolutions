using System;
using Microsoft.Practices.ServiceLocation;
using WB.Core.GenericSubdomains.Logging;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.SharedKernels.DataCollection;

namespace WB.Core.SharedKernels.SurveyManagement.Implementation.Accessors
{
    public class QuestionnaireAssemblyFileAccessor : IQuestionnareAssemblyFileAccessor
    {
        private const string FolderName = "QuestionnaireAssemblies";
        private readonly IFileSystemAccessor fileSystemAccessor;
        private readonly string pathToStore;

        private static ILogger Logger
        {
            get { return ServiceLocator.Current.GetInstance<ILogger>(); }
        }

        public QuestionnaireAssemblyFileAccessor(IFileSystemAccessor fileSystemAccessor, string folderPath)
        {
            this.fileSystemAccessor = fileSystemAccessor;

            this.pathToStore = fileSystemAccessor.CombinePath(folderPath, FolderName);
            if (!fileSystemAccessor.IsDirectoryExists(this.pathToStore))
                fileSystemAccessor.CreateDirectory(this.pathToStore);
        }

        public string GetFullPathToAssembly(Guid questionnaireId, long questionnaireVersion)
        {
            string assemblyFileName = this.GetAssemblyFileName(questionnaireId, questionnaireVersion);
            return this.fileSystemAccessor.CombinePath(this.pathToStore, assemblyFileName);
        }

        public void StoreAssembly(Guid questionnaireId, long questionnaireVersion, string assemblyAsBase64)
        {
            string assemblyFileName = this.GetAssemblyFileName(questionnaireId, questionnaireVersion);
            var pathToSaveAssembly = this.fileSystemAccessor.CombinePath(this.pathToStore, assemblyFileName);

            if (this.fileSystemAccessor.IsFileExists(pathToSaveAssembly))
                this.fileSystemAccessor.DeleteFile(pathToSaveAssembly);

            this.fileSystemAccessor.WriteAllBytes(pathToSaveAssembly, Convert.FromBase64String(assemblyAsBase64));
        }

        public void RemoveAssembly(Guid questionnaireId, long questionnaireVersion)
        {
            string assemblyFileName = this.GetAssemblyFileName(questionnaireId, questionnaireVersion);
            var pathToSaveAssembly = this.fileSystemAccessor.CombinePath(this.pathToStore, assemblyFileName);

            //loaded assembly could be locked
            try
            {
                this.fileSystemAccessor.DeleteFile(pathToSaveAssembly);
            }
            catch (Exception e)
            {
                Logger.Error("Error on assembly deletion");
                Logger.Error(e.Message, e);
            }
        }

        public string GetAssemblyAsBase64String(Guid questionnaireId, long questionnaireVersion)
        {
            byte[] assemblyAsByteArray = this.GetAssemblyAsByteArray(questionnaireId, questionnaireVersion);

            return Convert.ToBase64String(assemblyAsByteArray);
        }

        public byte[] GetAssemblyAsByteArray(Guid questionnaireId, long questionnaireVersion)
        {
            string assemblyFileName = this.GetAssemblyFileName(questionnaireId, questionnaireVersion);
            string pathToAssembly = this.fileSystemAccessor.CombinePath(this.pathToStore, assemblyFileName);

            return this.fileSystemAccessor.ReadAllBytes(pathToAssembly);
        }

        private string GetAssemblyFileName(Guid questionnaireId, long questionnaireVersion)
        {
            return String.Format("assembly_{0}_v{1}", questionnaireId, questionnaireVersion);
        }
    }
}