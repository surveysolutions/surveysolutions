using System;
using System.Linq;

using WB.Core.Infrastructure.FileSystem;
using WB.Core.SharedKernels.DataCollection.Implementation.Accessors;

namespace WB.Core.BoundedContexts.QuestionnaireTester.Implementation.Services
{
    public class TesterQuestionnaireAssemblyFileAccessor : IQuestionnaireAssemblyFileAccessor
    {
        private readonly IFileSystemAccessor fileSystemAccessor;
        
        private readonly string assemblyStorageDirectory;

        public TesterQuestionnaireAssemblyFileAccessor(string assemblyStorageDirectory, IFileSystemAccessor fileSystemAccessor)
        {
            this.fileSystemAccessor = fileSystemAccessor;
            this.assemblyStorageDirectory = assemblyStorageDirectory;
        }

        public string GetFullPathToAssembly(Guid questionnaireId, long questionnaireVersion)
        {
            return GetFullPathToAssembly(questionnaireId);
        }

        public void StoreAssembly(Guid questionnaireId, long questionnaireVersion, string assemblyAsBase64String)
        {
            string folderName = GetFolderNameForTemplate(questionnaireId);
            string pathToFolder = fileSystemAccessor.CombinePath(this.assemblyStorageDirectory, folderName);

            //version doesn't have sense to the tester
            //we are trying to delete old versions before saving the last one
            if (!fileSystemAccessor.IsDirectoryExists(pathToFolder))
            {
                fileSystemAccessor.CreateDirectory(pathToFolder);
            }
            else
            {
                foreach (var file in fileSystemAccessor.GetFilesInDirectory(pathToFolder))
                {
                    try
                    {
                        fileSystemAccessor.DeleteFile(file);
                    }
                    catch
                    {
                        //ignore locked files
                    }
                }
            }

            //generate unique new file name due to version is not valid for tester
            var fileName = string.Format("{0}.dll", Guid.NewGuid());
            var pathToSaveAssembly = fileSystemAccessor.CombinePath(pathToFolder, fileName);

            fileSystemAccessor.WriteAllBytes(pathToSaveAssembly, Convert.FromBase64String(assemblyAsBase64String));

            fileSystemAccessor.MarkFileAsReadonly(pathToSaveAssembly);  
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
            var assemblyPath = GetFullPathToAssembly(questionnaireId);
            if (!fileSystemAccessor.IsFileExists(assemblyPath))
                return null;

            return fileSystemAccessor.ReadAllBytes(GetFullPathToAssembly(questionnaireId));
        }

        private string GetFolderNameForTemplate(Guid questionnaireId)
        {
            return String.Format("dir-{0}", questionnaireId);
        }

        private string GetFullPathToAssembly(Guid questionnaireId)
        {
            var folderName = GetFolderNameForTemplate(questionnaireId);
            var assemblySearchPath = fileSystemAccessor.CombinePath(this.assemblyStorageDirectory, folderName);
            var filesInDirectory = fileSystemAccessor.GetFilesInDirectory(assemblySearchPath);

            return filesInDirectory.OrderByDescending(f => fileSystemAccessor.GetCreationTime(f)).First();
        }
    }
}
