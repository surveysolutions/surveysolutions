using System;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Commands.Interview;

namespace WB.UI.Headquarters.API.WebInterview
{
    public partial class WebInterview
    {
        private Guid commandResponsibleId
            => this.webInterviewConfigProvider.Get(this.GetCallerInterview().QuestionnaireIdentity).ResponsibleId;

        public void AnswerTextQuestion(string questionIdenty, string text)
        {
            var identity = Identity.Parse(questionIdenty);
            this.commandService.Execute(new AnswerTextQuestionCommand(this.GetCallerInterview().Id,
                this.commandResponsibleId, identity.Id, identity.RosterVector, DateTime.UtcNow, text));
        }

        public void AnswerSingleOptionQuestion(int answer, string questionId)
        {
            Identity identity = Identity.Parse(questionId);
            this.commandService.Execute(new AnswerSingleOptionQuestionCommand(this.GetCallerInterview().Id, commandResponsibleId,
                identity.Id, identity.RosterVector, DateTime.UtcNow, answer));
        }
        public void AnswerIntegerQuestion(string questionIdenty, int answer)
        {
            Identity identity = Identity.Parse(questionIdenty);
            var command = new AnswerNumericIntegerQuestionCommand(this.GetCallerInterview().Id, commandResponsibleId, identity.Id, identity.RosterVector, DateTime.UtcNow, answer);
            this.commandService.Execute(command);
        }
        public void AnswerDoubleQuestion(string questionIdenty, double answer)
        {
            Identity identity = Identity.Parse(questionIdenty);
            var command = new AnswerNumericRealQuestionCommand(this.GetCallerInterview().Id, commandResponsibleId, identity.Id, identity.RosterVector, DateTime.UtcNow, answer);
            this.commandService.Execute(command);
        }

        public void RemoveAnswer(string questionId)
        {
            Identity identity = Identity.Parse(questionId);
            this.commandService.Execute(new RemoveAnswerCommand(this.GetCallerInterview().Id, commandResponsibleId, identity,
                DateTime.UtcNow));
        }
    }
}