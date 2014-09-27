using System;
using System.IO;
using WB.Core.Infrastructure.Backup;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.SharedKernels.DataCollection;
using Environment = Android.OS.Environment;

namespace CAPI.Android.Core.Model
{
    public class QuestionnareAssemblyCapiFileAccessor : IQuestionnaireAssemblyFileAccessor, IBackupable
    {
        private const string storeName = "QuestionnaireAssemblies";
        private readonly string pathToStore;
        private readonly IFileSystemAccessor fileSystemAccessor;

        public QuestionnareAssemblyCapiFileAccessor(IFileSystemAccessor fileSystemAccessor)
        {
            this.fileSystemAccessor = fileSystemAccessor;

            var storageDirectory = fileSystemAccessor.IsDirectoryExists(global::Android.OS.Environment.ExternalStorageDirectory.AbsolutePath)
                             ? Environment.ExternalStorageDirectory.AbsolutePath
                             : System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal);

            storageDirectory = fileSystemAccessor.CombinePath(storageDirectory, storeName);

            if (!fileSystemAccessor.IsDirectoryExists(storageDirectory))
            {
                fileSystemAccessor.CreateDirectory(storageDirectory);
            }

            this.pathToStore = storageDirectory;
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
            }
        }

        public string GetAssemblyAsBase64String(Guid questionnaireId, long questionnaireVersion)
        {
            throw new NotImplementedException();
        }

        public byte[] GetAssemblyAsByteArray(Guid questionnaireId, long questionnaireVersion)
        {
            throw new NotImplementedException();
        }

        private string GetAssemblyFileName(Guid questionnaireId, long questionnaireVersion)
        {
            return String.Format("assembly_{0}_v{1}.dll", questionnaireId, questionnaireVersion);
        }

        public string GetPathToBackupFile()
        {
            return pathToStore;
        }

        public void RestoreFromBackupFolder(string path)
        {
            var dirWithCahngelog = fileSystemAccessor.CombinePath(path, storeName);

            foreach (var file in Directory.EnumerateFiles(this.pathToStore))
            {
                File.Delete(file);
            }

            if (!Directory.Exists(dirWithCahngelog))
                return;

            foreach (var file in Directory.GetFiles(dirWithCahngelog))
                File.Copy(file, Path.Combine(this.pathToStore, Path.GetFileName(file)));
        }
    }
}