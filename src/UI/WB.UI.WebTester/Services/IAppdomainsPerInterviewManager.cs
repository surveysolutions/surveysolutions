using System;
using System.Collections.Generic;
using Main.Core.Documents;
using Ncqrs.Eventing;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.SharedKernels.DataCollection;

namespace WB.UI.WebTester.Services
{
    public interface IAppdomainsPerInterviewManager
    {
        void SetupForInterview(Guid interviewId, QuestionnaireDocument questionnaireDocument, string supportingAssembly);
        void TearDown(Guid interviewId);
        List<CommittedEvent> Execute(ICommand command);
        
        List<CategoricalOption> GetFirstTopFilteredOptionsForQuestion(Guid interviewId, Identity questionIdentity, 
            int? parentQuestionValue, string filter, int itemsCount = 200);

        CategoricalOption GetOptionForQuestionWithFilter(Guid interviewId, Identity question,
            string optionText, int? parentQuestionValue = null);
    }
}