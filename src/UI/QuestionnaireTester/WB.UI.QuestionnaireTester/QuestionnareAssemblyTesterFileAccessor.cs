using System;
using System.IO;
using System.Linq;
using WB.Core.SharedKernels.DataCollection;
using Environment = Android.OS.Environment;

namespace WB.UI.QuestionnaireTester
{
    public class QuestionnareAssemblyTesterFileAccessor : IQuestionnareAssemblyFileAccessor
    {
        private const string storeName = "ExpressionState";
        private readonly string pathToStore;

        public QuestionnareAssemblyTesterFileAccessor()
        {
            var storageDirectory = Directory.Exists(Android.OS.Environment.ExternalStorageDirectory.AbsolutePath)
                             ? Environment.ExternalStorageDirectory.AbsolutePath
                             : System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal);

            storageDirectory = Path.Combine(storageDirectory, storeName);

            if (!Directory.Exists(storageDirectory))
            {
                Directory.CreateDirectory(storageDirectory);
            }

            pathToStore = storageDirectory;
        }


        public string GetFullPathToAssembly(Guid questionnaireId, long questionnaireVersion)
        {
            var folderName = GetFolderNameForTemplate(questionnaireId, questionnaireVersion);
            var assemblySearchPath = Path.Combine(pathToStore, folderName);
            var directory = new DirectoryInfo(assemblySearchPath);

            //temporary solution to locate assembly for tester
            string assemblyFilePath = directory.GetFiles().OrderByDescending(f => f.LastWriteTime).First().FullName;

            return assemblyFilePath;
        }

        public void StoreAssembly(Guid questionnaireId, long questionnaireVersion, string assemblyAsBase64String)
        {
            string folderName = GetFolderNameForTemplate(questionnaireId, questionnaireVersion);
            string pathToFolder = Path.Combine(pathToStore, folderName);

            //version doesn't have scence to the tester
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
            var pathToSaveAssembly = Path.Combine(pathToFolder, Guid.NewGuid().ToString());

            File.WriteAllBytes(pathToSaveAssembly, Convert.FromBase64String(assemblyAsBase64String));
        }

        public void RemoveAssembly(Guid questionnaireId, long questionnaireVersion)
        {
            throw new NotImplementedException();
        }

        public string GetAssemblyAsBase64String(Guid questionnaireId, long questionnaireVersion)
        {
            throw new NotImplementedException();
        }

        public byte[] GetAssemblyAsByteArray(Guid questionnaireId, long questionnaireVersion)
        {
            throw new NotImplementedException();
        }

        private string GetFolderNameForTemplate(Guid questionnaireId, long questionnaireVersion)
        {
            return String.Format("dir-{0}-{1}", questionnaireId, questionnaireVersion);
        }
    }
}