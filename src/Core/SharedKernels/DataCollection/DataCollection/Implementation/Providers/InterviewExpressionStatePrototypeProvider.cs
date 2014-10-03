using System;
using System.Linq;
using System.Reflection;
using Microsoft.Practices.ServiceLocation;
using WB.Core.GenericSubdomains.Logging;
using WB.Core.SharedKernels.DataCollection.Exceptions;
using WB.Core.SharedKernels.DataCollection.Implementation.Accessors;

namespace WB.Core.SharedKernels.DataCollection.Implementation.Providers
{
    public class InterviewExpressionStatePrototypeProvider : IInterviewExpressionStatePrototypeProvider
    {
        private static ILogger Logger
        {
            get { return ServiceLocator.Current.GetInstance<ILogger>(); }
        }

        private readonly IQuestionnaireAssemblyFileAccessor questionnareAssemblyFileAccessor;

        public InterviewExpressionStatePrototypeProvider(IQuestionnaireAssemblyFileAccessor questionnareAssemblyFileAccessor)
        {
            this.questionnareAssemblyFileAccessor = questionnareAssemblyFileAccessor;
        }

        public IInterviewExpressionState GetExpressionState(Guid questionnaireId, long questionnaireVersion)
        {
            string assemblyFile = this.questionnareAssemblyFileAccessor.GetFullPathToAssembly(questionnaireId, questionnaireVersion);

            try
            {
                //path is cached
                //if assembly was loaded from this path it won't be loaded again 
                var compiledAssembly = Assembly.LoadFrom(assemblyFile);

                Type interviewExpressionStateType = compiledAssembly.GetTypes().
                    SingleOrDefault(type => !(type.IsAbstract || type.IsGenericTypeDefinition || type.IsInterface) && type.GetInterfaces().Contains(typeof(IInterviewExpressionState)));

                if (interviewExpressionStateType == null)
                    throw new Exception("Type implementing IInterviewExpressionState was not found");

                var interviewExpressionState = Activator.CreateInstance(interviewExpressionStateType) as IInterviewExpressionState;

                return interviewExpressionState;
            }
            catch (Exception exception)
            {
                Logger.Fatal("Error on assembly loading", exception);
                //hide original one
                throw new InterviewException("Interview loading error. Code EC0001");
            }

            
        }
    }
}
