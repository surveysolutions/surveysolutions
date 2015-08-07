using System;
using System.Linq;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.SharedKernels.DataCollection.Implementation.Accessors;

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

        public string GetFullPathToAssembly(Guid questionnaireId, long questionnaireVersion)
        {
            return this.GetFullPathToAssembly(questionnaireId);
        }

        public void StoreAssembly(Guid questionnaireId, long questionnaireVersion, string assemblyAsBase64String)
        {
            string folderName = this.GetFolderNameForTemplate(questionnaireId);
            string pathToFolder = this.fileSystemAccessor.CombinePath(this.assemblyStorageDirectory, folderName);

            //version doesn't have sense to the tester
            //we are trying to delete old versions before saving the last one
            if (!this.fileSystemAccessor.IsDirectoryExists(pathToFolder))
            {
                this.fileSystemAccessor.CreateDirectory(pathToFolder);
            }
            else
            {
                foreach (var file in this.fileSystemAccessor.GetFilesInDirectory(pathToFolder))
                {
                    try
                    {
                        this.fileSystemAccessor.DeleteFile(file);
                    }
                    catch
                    {
                        //ignore locked files
                    }
                }
            }

            //generate unique new file name due to version is not valid for tester
            var fileName = string.Format("{0}.dll", Guid.NewGuid());
            var pathToSaveAssembly = this.fileSystemAccessor.CombinePath(pathToFolder, fileName);

            this.fileSystemAccessor.WriteAllBytes(pathToSaveAssembly, Convert.FromBase64String(assemblyAsBase64String));

            this.fileSystemAccessor.MarkFileAsReadonly(pathToSaveAssembly);  
        }

        public void RemoveAssembly(Guid questionnaireId, long questionnaireVersion)
        {
            
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
            var filesInDirectory = this.fileSystemAccessor.GetFilesInDirectory(assemblySearchPath);

            return filesInDirectory.OrderByDescending(f => this.fileSystemAccessor.GetCreationTime(f)).First();
        }
    }
}
