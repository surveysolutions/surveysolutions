using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.IO;
using Java.Lang;
using WB.Core.SharedKernels.DataCollection;
using Environment = Android.OS.Environment;
using Exception = System.Exception;
using String = System.String;

namespace WB.UI.QuestionnaireTester
{
    public class InterviewExpressionStateTesterProvider : IInterviewExpressionStateProvider
    {
        private string storeName = "ExpressionState";
        private string pathToStore;
        private Dictionary<string, IInterviewExpressionState> cache = new Dictionary<string, IInterviewExpressionState>();

        public InterviewExpressionStateTesterProvider()
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

        public IInterviewExpressionState GetExpressionState(Guid questionnaireId, long questionnaireVersion)
        {
            string folderName = GetFolderName(questionnaireId, questionnaireVersion);

            if (!cache.ContainsKey(folderName))
            {
                var assemblySearchPath = Path.Combine(pathToStore, folderName);

                var directory = new DirectoryInfo(assemblySearchPath);

                //temporary solution to locate assembly
                string assemblyFile = directory.GetFiles().OrderByDescending(f => f.LastWriteTime).First().FullName;
                
                var compiledAssembly = Assembly.LoadFrom(assemblyFile);
                Type interviewExpressionStateType = compiledAssembly.GetTypes().FirstOrDefault(type => type.GetInterfaces().Contains(typeof(IInterviewExpressionState)));

                if(interviewExpressionStateType == null)
                    throw new Exception("Type impementing IInterviewExpressionState was not found");
                
                var interviewExpressionState = Activator.CreateInstance(interviewExpressionStateType) as IInterviewExpressionState;
                cache.Add(folderName, interviewExpressionState);
            }

            //assuming reflection activation is slower then cloning
            return cache[folderName].Clone();
        }

        public void StoreAssembly(Guid questionnaireId, long questionnaireVersion, string assembly)
        {
            string folderName = GetFolderName(questionnaireId, questionnaireVersion);

            cache.Remove(folderName);
            string pathToFolder = Path.Combine(pathToStore, folderName);
            
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

            File.WriteAllBytes(Path.Combine(pathToFolder, Guid.NewGuid().ToString()), Convert.FromBase64String(assembly));
        }

        private string GetFolderName(Guid questionnaireId, long questionnaireVersion)
        {
            return String.Format("dir-{0}-{1}", questionnaireId, questionnaireVersion);
        }
    }
}