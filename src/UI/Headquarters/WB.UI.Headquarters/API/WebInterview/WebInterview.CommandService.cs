using System;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Commands.Interview;
using WB.Core.SharedKernels.DataCollection.Commands.Interview.Base;

namespace WB.UI.Headquarters.API.WebInterview
{
    public partial class WebInterview
    {
        private Guid commandResponsibleId => this.webInterviewConfigProvider.Get(this.GetCallerInterview().QuestionnaireIdentity).ResponsibleId;

        private void ExecuteCommand(QuestionCommand command)
        {
            try
            {
                this.commandService.Execute(command);
            }
            catch (Exception e)
            {
                this.Clients.Caller.markAnswerAsNotSaved(command.QuestionId.FormatGuid(), e.Message);
            }
        }

        public void AnswerTextQuestion(string questionIdenty, string text)
        {
            var identity = Identity.Parse(questionIdenty);
            ExecuteCommand(new AnswerTextQuestionCommand(this.GetCallerInterview().Id,
                this.commandResponsibleId, identity.Id, identity.RosterVector, DateTime.UtcNow, text));
        }

        public void AnswerDateQuestion(string questionIdenty, DateTime answer)
        {
            var identity = Identity.Parse(questionIdenty);
            ExecuteCommand(new AnswerDateTimeQuestionCommand(this.GetCallerInterview().Id,
                this.commandResponsibleId, identity.Id, identity.RosterVector, DateTime.UtcNow, answer));
        }

        public void AnswerSingleOptionQuestion(int answer, string questionId)
        {
            Identity identity = Identity.Parse(questionId);
            ExecuteCommand(new AnswerSingleOptionQuestionCommand(this.GetCallerInterview().Id, commandResponsibleId,
                identity.Id, identity.RosterVector, DateTime.UtcNow, answer));
        }

        public void AnswerMultiOptionQuestion(int[] answer, string questionId)
        {
            Identity identity = Identity.Parse(questionId);
            ExecuteCommand(new AnswerMultipleOptionsQuestionCommand(this.GetCallerInterview().Id, commandResponsibleId,
                identity.Id, identity.RosterVector, DateTime.UtcNow, answer));
        }

        public void AnswerIntegerQuestion(string questionIdenty, int answer)
        {
            Identity identity = Identity.Parse(questionIdenty);
            ExecuteCommand(new AnswerNumericIntegerQuestionCommand(this.GetCallerInterview().Id, this.commandResponsibleId, identity.Id, identity.RosterVector, DateTime.UtcNow, answer));
        }

        public void AnswerDoubleQuestion(string questionIdenty, double answer)
        {
            Identity identity = Identity.Parse(questionIdenty);
            ExecuteCommand(new AnswerNumericRealQuestionCommand(this.GetCallerInterview().Id, this.commandResponsibleId, identity.Id, identity.RosterVector, DateTime.UtcNow, answer));
        }

        public void RemoveAnswer(string questionId)
        {
            Identity identity = Identity.Parse(questionId);
            ExecuteCommand(new RemoveAnswerCommand(this.GetCallerInterview().Id, commandResponsibleId, identity, DateTime.UtcNow));
        }
    }
}