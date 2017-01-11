using System;
using System.CodeDom;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Commands.Interview;
using WB.Core.SharedKernels.DataCollection.Exceptions;
using WB.UI.Headquarters.Code.CommandTransformation;

namespace WB.UI.Headquarters.API.WebInterview
{
    public partial class WebInterview
    {
        private Guid commandResponsibleId
            => this.webInterviewConfigProvider.Get(this.currentInterview.QuestionnaireIdentity).ResponsibleId;

        public void AnswerTextQuestion(string questionIdenty, string text)
        {
            var identity = Identity.Parse(questionIdenty);
            this.commandService.Execute(new AnswerTextQuestionCommand(this.currentInterview.Id,
                this.commandResponsibleId, identity.Id, identity.RosterVector, DateTime.UtcNow, text));
        }

        public void AnswerSingleOptionQuestion(int answer, string questionId)
        {
            Identity identity = Identity.Parse(questionId);
            var command = new AnswerSingleOptionQuestionCommand(currentInterview.Id, commandResponsibleId, identity.Id, identity.RosterVector, DateTime.UtcNow, answer);
            this.commandService.Execute(command);
        }

        public void RemoveAnswer(string questionId)
        {
            Identity identity = Identity.Parse(questionId);
            var command = new RemoveAnswerCommand(currentInterview.Id, commandResponsibleId, identity, DateTime.UtcNow);
            this.commandService.Execute(command);
        }
    }
}