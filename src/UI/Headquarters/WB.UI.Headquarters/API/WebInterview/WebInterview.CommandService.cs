using System;
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

        public void ExecuteCommand(string commandType, string command)
        {
            try
            {
                ICommand concreteCommand = this.commandDeserializer.Deserialize(commandType, command);
                ICommand transformedCommand = new CommandTransformator().TransformCommnadIfNeeded(concreteCommand, this.commandResponsibleId);
                this.commandService.Execute(transformedCommand);
            }
            catch (OverflowException e)
            {
                this.logger.Error(e.Message, e);
                this.Clients.Caller.unhandledException();
            }
            catch (Exception e)
            {
                var interviewException = e.GetSelfOrInnerAs<InterviewException>();
                if (interviewException != null)
                    this.Clients.Caller.interviewException(interviewException);

                var userException = e.GetSelfOrInnerAs<UserException>();
                if (userException != null)
                    this.Clients.Caller.userException(userException);

                if (interviewException == null && userException == null)
                {
                    this.logger.Error(e.Message, e);
                    this.Clients.Caller.unhandledException();
                }
            }
        }

        public void AnswerSingleOptionQuestion(int answer, string questionId)
        {
            Identity identity = Identity.Parse(questionId);
            var command = new AnswerSingleOptionQuestionCommand(currentInterview.Id, commandResponsibleId, identity.Id, identity.RosterVector, DateTime.Now, answer);
            this.commandService.Execute(command);
        }
    }
}