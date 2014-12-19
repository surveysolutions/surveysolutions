using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.Practices.ServiceLocation;
using WB.Core.GenericSubdomains.Logging;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.SharedKernels.DataCollection.Exceptions;
using WB.Core.SharedKernels.DataCollection.Implementation.Accessors;

namespace WB.Core.SharedKernels.DataCollection.Implementation.Providers
{
    internal class InterviewExpressionStatePrototypeProvider : IInterviewExpressionStatePrototypeProvider, IDisposable
    {
        private readonly Dictionary<string, string> tempAssemblies = new Dictionary<string, string>(); 

        private static ILogger Logger
        {
            get { return ServiceLocator.Current.GetInstance<ILogger>(); }
        }

        private readonly IQuestionnaireAssemblyFileAccessor questionnareAssemblyFileAccessor;
        private readonly IFileSystemAccessor fileSystemAccessor;

        public InterviewExpressionStatePrototypeProvider(IQuestionnaireAssemblyFileAccessor questionnareAssemblyFileAccessor, IFileSystemAccessor fileSystemAccessor)
        {
            this.questionnareAssemblyFileAccessor = questionnareAssemblyFileAccessor;
            this.fileSystemAccessor = fileSystemAccessor;

        }

        public IInterviewExpressionState GetExpressionState(Guid questionnaireId, long questionnaireVersion)
        {
            string assemblyFile = this.questionnareAssemblyFileAccessor.GetFullPathToAssembly(questionnaireId, questionnaireVersion);

            if (!fileSystemAccessor.IsFileExists(assemblyFile))
            {
                Logger.Fatal(String.Format("Assembly was not found. Questionnaire={0}, version={1}, search={2}",
                    questionnaireId, questionnaireVersion, assemblyFile));
                throw new InterviewException("Interview loading error. Code EC0003");
            }

            if (!this.tempAssemblies.ContainsKey(assemblyFile))
            {
                string tempFileName = this.fileSystemAccessor.GetTempFile();
                this.fileSystemAccessor.CopyFile(assemblyFile, tempFileName);
                this.tempAssemblies[assemblyFile] = tempFileName;
            }

            try
            {
                // path is cached
                // if assembly was loaded from this path it won't be loaded again 
                var compiledAssembly = fileSystemAccessor.LoadAssembly(this.tempAssemblies[assemblyFile]);

                var interviewExpressionStateTypeInfo = compiledAssembly.DefinedTypes.
                    SingleOrDefault(x => !(x.IsAbstract || x.IsGenericTypeDefinition || x.IsInterface) && x.ImplementedInterfaces.Contains(typeof(IInterviewExpressionState)));

                if (interviewExpressionStateTypeInfo == null)
                    throw new InvalidOperationException("Type implementing " + typeof(IInterviewExpressionState) + "IInterviewExpressionState was not found in assembly " + assemblyFile);

                Type interviewExpressionStateType = interviewExpressionStateTypeInfo.AsType();
                try
                {
                    var interviewExpressionState = Activator.CreateInstance(interviewExpressionStateType) as IInterviewExpressionState;

                    return interviewExpressionState;
                }
                catch (Exception e)
                {
                    Logger.Fatal("Error on activating interview expression state. Cannot cast to created object to IInterviewExpressionState", e);
                    return null;
                }
            }
            catch (Exception exception)
            {
                Logger.Fatal(String.Format("Error on assembly loading for id={0} version={1}", questionnaireId, questionnaireVersion), exception);
                if (exception.InnerException != null)
                    Logger.Fatal("Error on assembly loading (inner)", exception.InnerException);

                //hide original one
                throw new InterviewException("Interview loading error. Code EC0001");
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        
        ~InterviewExpressionStatePrototypeProvider()
        {
            Dispose(false);
        }
       
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                // free managed resources
                
            }

            foreach (var tempFolder in tempAssemblies.Values)
            {
                this.fileSystemAccessor.DeleteDirectory(tempFolder);
            }
        }
    }
}
