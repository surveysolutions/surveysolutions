using System;
using System.Collections.Generic;
using Main.Core.Documents;
using Ncqrs.Eventing;
using WB.Core.Infrastructure.CommandBus;

namespace WB.UI.WebTester.Services
{
    public interface IAppdomainsPerInterviewManager
    {
        void SetupForInterview(Guid interviewId, QuestionnaireDocument questionnaireDocument, string supportingAssembly);
        void Dispose(Guid interviewId);
        List<CommittedEvent> Execute(ICommand command);
    }
}