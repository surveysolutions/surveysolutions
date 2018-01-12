using System;
using Main.Core.Documents;

namespace WB.UI.WebTester.Services
{
    public interface IAppdomainsPerInterviewManager
    {
        void SetupForInterview(Guid interviewId, QuestionnaireDocument questionnaireDocument, string supportingAssembly);
        void Dispose(Guid interviewId);
    }
}