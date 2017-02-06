using System;
using System.Linq;
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
            if (!this.fileSystemAccessor.IsFileExists(assemblyPath))
                return null;

            return this.fileSystemAccessor.ReadAllBytes(assemblyPath);
        }

        private string GetFolderNameForTemplate(Guid questionnaireId)
        {
            return String.Format("dir-{0}", questionnaireId);
        }

        public string CheckAndGetFullPathToAssemblyOrEmpty(Guid questionnaireId, long questionnaireVersion)
        {
            var folderName = this.GetFolderNameForTemplate(questionnaireId);
            var assemblySearchPath = this.fileSystemAccessor.CombinePath(this.assemblyStorageDirectory, folderName);
            var filesInDirectory = this.fileSystemAccessor.GetFilesInDirectory(assemblySearchPath);

            return filesInDirectory.OrderByDescending(f => this.fileSystemAccessor.GetCreationTime(f)).First();
        }

        public bool IsQuestionnaireAssemblyExists(Guid questionnaireId, long questionnaireVersion)
        {
            throw new NotImplementedException();
        }
    }
}
