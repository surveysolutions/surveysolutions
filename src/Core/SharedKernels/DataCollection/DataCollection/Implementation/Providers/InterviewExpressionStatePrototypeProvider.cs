using System;
using System.Linq;
using System.Reflection;
using Microsoft.Practices.ServiceLocation;
using WB.Core.GenericSubdomains.Utils.Services;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.SharedKernels.DataCollection.Exceptions;
using WB.Core.SharedKernels.DataCollection.Implementation.Accessors;
using WB.Core.SharedKernels.DataCollection.V2;

namespace WB.Core.SharedKernels.DataCollection.Implementation.Providers
{
    internal class InterviewExpressionStatePrototypeProvider : IInterviewExpressionStatePrototypeProvider
    {
        private static ILogger Logger
        {
            get { return ServiceLocator.Current.GetInstance<ILogger>(); }
        }

        private readonly IQuestionnaireAssemblyFileAccessor questionnareAssemblyFileAccessor;
        private readonly IFileSystemAccessor fileSystemAccessor;
        private readonly IInterviewExpressionStateVersionAdapter interviewExpressionStateVersionAdapter;

        public InterviewExpressionStatePrototypeProvider(IQuestionnaireAssemblyFileAccessor questionnareAssemblyFileAccessor, IFileSystemAccessor fileSystemAccessor, IInterviewExpressionStateVersionAdapter interviewExpressionStateVersionAdapter)
        {
            this.questionnareAssemblyFileAccessor = questionnareAssemblyFileAccessor;
            this.fileSystemAccessor = fileSystemAccessor;
            this.interviewExpressionStateVersionAdapter = interviewExpressionStateVersionAdapter;
        }

        public IInterviewExpressionStateV2 GetExpressionState(Guid questionnaireId, long questionnaireVersion)
        {
            string assemblyFile = this.questionnareAssemblyFileAccessor.GetFullPathToAssembly(questionnaireId, questionnaireVersion);

            if (!fileSystemAccessor.IsFileExists(assemblyFile))
            {
                Logger.Fatal(String.Format("Assembly was not found. Questionnaire={0}, version={1}, search={2}", 
                    questionnaireId, questionnaireVersion, assemblyFile));
                throw new InterviewException("Interview loading error. Code EC0003");
            }

            try
            {
                //path is cached
                //if assembly was loaded from this path it won't be loaded again 
                var compiledAssembly = fileSystemAccessor.LoadAssembly(assemblyFile);

                TypeInfo interviewExpressionStateTypeInfo = compiledAssembly.DefinedTypes.
                    SingleOrDefault(x => !(x.IsAbstract || x.IsGenericTypeDefinition || x.IsInterface) && x.ImplementedInterfaces.Contains(typeof (IInterviewExpressionState)));

                if (interviewExpressionStateTypeInfo == null)
                    throw new Exception("Type implementing IInterviewExpressionState was not found");

                Type interviewExpressionStateType = interviewExpressionStateTypeInfo.AsType();
                try
                {
                    return interviewExpressionStateVersionAdapter.AdaptToV2(Activator.CreateInstance(interviewExpressionStateType) as IInterviewExpressionState);
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
    }
}
