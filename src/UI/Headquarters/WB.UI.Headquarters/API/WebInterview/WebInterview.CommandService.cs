using System;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.SharedKernels.DataCollection.Exceptions;
using WB.UI.Headquarters.Code.CommandTransformation;

namespace WB.UI.Headquarters.API.WebInterview
{
    public partial class WebInterview
    {
        public void ExecuteCommand(string commandType, string command)
        {
            try
            {
                ICommand concreteCommand = this.commandDeserializer.Deserialize(commandType, command);
                ICommand transformedCommand = new CommandTransformator().TransformCommnadIfNeeded(concreteCommand);
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
    }
}