using System;
using System.Linq;
using System.Reflection;
using WB.Core.SharedKernels.DataCollection;


namespace WB.Core.SharedKernels.SurveyManagement.Implementation.QuestionnaireAssembly
{
    public class InterviewExpressionStateProvider : IInterviewExpressionStateProvider
    {
        private readonly IQuestionnareAssemblyFileAccessor questionnareAssemblyFileAccessor;

        public InterviewExpressionStateProvider(IQuestionnareAssemblyFileAccessor questionnareAssemblyFileAccessor)
        {
            this.questionnareAssemblyFileAccessor = questionnareAssemblyFileAccessor;
        }

        public IInterviewExpressionState GetExpressionState(Guid questionnaireId, long questionnaireVersion)
        {
            string assemblyFile = questionnareAssemblyFileAccessor.GetFullPathToAssembly(questionnaireId, questionnaireVersion);
            
            //path is chached
            //if assembly was loaded from this path it won't be loaded again 
            var compiledAssembly = Assembly.LoadFrom(assemblyFile);

            Type interviewExpressionStateType = compiledAssembly.GetTypes().FirstOrDefault(type => type.GetInterfaces().Contains(typeof(IInterviewExpressionState)));

            if (interviewExpressionStateType == null)
                throw new Exception("Type impementing IInterviewExpressionState was not found");

            var interviewExpressionState = Activator.CreateInstance(interviewExpressionStateType) as IInterviewExpressionState;

            return interviewExpressionState;
        }
    }
}
