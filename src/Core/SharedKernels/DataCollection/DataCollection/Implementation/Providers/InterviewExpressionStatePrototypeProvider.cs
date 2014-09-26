using System;
using System.Linq;
using System.Reflection;

namespace WB.Core.SharedKernels.DataCollection.Implementation.Providers
{
    public class InterviewExpressionStatePrototypeProvider : IInterviewExpressionStatePrototypeProvider
    {
        private readonly IQuestionnareAssemblyFileAccessor questionnareAssemblyFileAccessor;

        public InterviewExpressionStatePrototypeProvider(IQuestionnareAssemblyFileAccessor questionnareAssemblyFileAccessor)
        {
            this.questionnareAssemblyFileAccessor = questionnareAssemblyFileAccessor;
        }

        public IInterviewExpressionState GetExpressionState(Guid questionnaireId, long questionnaireVersion)
        {
            string assemblyFile = this.questionnareAssemblyFileAccessor.GetFullPathToAssembly(questionnaireId, questionnaireVersion);
            
            //path is chached
            //if assembly was loaded from this path it won't be loaded again 
            var compiledAssembly = Assembly.LoadFrom(assemblyFile);

            Type interviewExpressionStateType = compiledAssembly.GetTypes().
                SingleOrDefault(type => !(type.IsAbstract || type.IsGenericTypeDefinition || type.IsInterface) && type.GetInterfaces().Contains(typeof(IInterviewExpressionState)));

            if (interviewExpressionStateType == null)
                throw new Exception("Type impementing IInterviewExpressionState was not found");

            var interviewExpressionState = Activator.CreateInstance(interviewExpressionStateType) as IInterviewExpressionState;

            return interviewExpressionState;
        }
    }
}
