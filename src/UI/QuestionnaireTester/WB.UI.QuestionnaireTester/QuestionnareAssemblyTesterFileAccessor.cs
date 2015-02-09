using System;
using System.IO;
using System.Linq;
using WB.Core.SharedKernels.DataCollection.Implementation.Accessors;
using Environment = Android.OS.Environment;

namespace WB.UI.QuestionnaireTester
{
    internal class QuestionnareAssemblyTesterFileAccessor : IQuestionnaireAssemblyFileAccessor
    {
        private const string StoreName = "QuestionnaireAssemblies";
        private readonly string pathToStore;

        public QuestionnareAssemblyTesterFileAccessor()
        {
            var storageDirectory = Directory.Exists(Android.OS.Environment.ExternalStorageDirectory.AbsolutePath)
                             ? Environment.ExternalStorageDirectory.AbsolutePath
                             : System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal);

            storageDirectory = Path.Combine(storageDirectory, StoreName);

            if (!Directory.Exists(storageDirectory))
            {
                Directory.CreateDirectory(storageDirectory);
            }

            pathToStore = storageDirectory;
        }


        public string GetFullPathToAssembly(Guid questionnaireId, long questionnaireVersion)
        {
            return GetFullPathToAssembly(questionnaireId);
        }

        public void StoreAssembly(Guid questionnaireId, long questionnaireVersion, string assemblyAsBase64String)
        {
            string folderName = GetFolderNameForTemplate(questionnaireId);
            string pathToFolder = Path.Combine(pathToStore, folderName);

            //version doesn't have sense to the tester
            //we are trying to delete old versions before saving the last one
            if (!Directory.Exists(pathToFolder))
            {
                Directory.CreateDirectory(pathToFolder);
            }
            else
            {
                foreach (var file in Directory.GetFiles(pathToFolder))
                {
                    try
                    {
                        File.Delete(file);
                    }
                    catch
                    {
                        //ignore locked files
                    }
                }
            }

            //generate unique new file name due to version is not valid for tester
            var fileName = string.Format("{0}.dll", Guid.NewGuid());
            var pathToSaveAssembly = Path.Combine(pathToFolder, fileName);

            File.WriteAllBytes(pathToSaveAssembly, Convert.FromBase64String(assemblyAsBase64String));

            File.SetAttributes(pathToSaveAssembly, FileAttributes.ReadOnly);  
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
            if (!File.Exists(assemblyPath))
                return null;
            
            return File.ReadAllBytes(GetFullPathToAssembly(questionnaireId));
        }

        private string GetFolderNameForTemplate(Guid questionnaireId)
        {
            return String.Format("dir-{0}", questionnaireId);
        }

        private string GetFullPathToAssembly(Guid questionnaireId)
        {
            var folderName = GetFolderNameForTemplate(questionnaireId);
            var assemblySearchPath = Path.Combine(pathToStore, folderName);
            var directory = new DirectoryInfo(assemblySearchPath);

            return directory.GetFiles().OrderByDescending(f => f.LastWriteTime).First().FullName;

        }
    }
}