using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.SharedKernels.DataCollection.Commands.Interview;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Enumerator.Native.WebInterview.Pipeline;

namespace WB.UI.Headquarters.Code.WebInterview.Pipeline
{
    public class PauseResumePipelineModule : IPipelineModule
    {
        private readonly ICommandService commandService;
        private readonly IAuthorizedUser authorizedUser;

        public PauseResumePipelineModule(ICommandService commandService, IAuthorizedUser authorizedUser)
        {
            this.commandService = commandService;
            this.authorizedUser = authorizedUser;
        }

        public Task OnConnected(Hub hub)
        {
            var interviewId = hub.GetInterviewId();

            if (authorizedUser.IsInterviewer)
            {
                commandService.Execute(new ResumeInterviewCommand(interviewId, this.authorizedUser.Id,
                    AgentDeviceType.Web));

            }
            else if (authorizedUser.IsSupervisor)
            {
                commandService.Execute(new OpenInterviewBySupervisorCommand(interviewId, this.authorizedUser.Id,
                    AgentDeviceType.Web));
            }

            return Task.CompletedTask;
        }

        public Task OnDisconnected(Hub hub, Exception exception)
        {
            var interviewId = hub.GetInterviewId();

            if (authorizedUser.IsInterviewer)
            {
                commandService.Execute(new PauseInterviewCommand(interviewId, this.authorizedUser.Id));
            }
            else if (authorizedUser.IsSupervisor)
            {
                commandService.Execute(new CloseInterviewBySupervisorCommand(interviewId, this.authorizedUser.Id));
            }

            return Task.CompletedTask;
        }
    }
}
