using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.BoundedContexts.Headquarters.WebInterview;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.SharedKernels.DataCollection.Commands.Interview;
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

        public async Task OnConnected(Hub hub)
        {
            var interviewId = hub.GetInterviewId();

            if (authorizedUser.IsInterviewer)
            {
                await commandService.ExecuteAsync(new ResumeInterviewCommand(interviewId, this.authorizedUser.Id))
                    .ConfigureAwait(false);
            }
            else if (authorizedUser.IsSupervisor)
            {
                await commandService.ExecuteAsync(new OpenInterviewBySupervisorCommand(interviewId, this.authorizedUser.Id))
                    .ConfigureAwait(false);
            }
        }

        public async Task OnDisconnected(Hub hub, Exception exception)
        {
            var interviewId = hub.GetInterviewId();

            if (authorizedUser.IsInterviewer)
            {
                await commandService.ExecuteAsync(new PauseInterviewCommand(interviewId, this.authorizedUser.Id))
                    .ConfigureAwait(false);
            }
            else if (authorizedUser.IsSupervisor)
            {
                await commandService.ExecuteAsync(new CloseInterviewBySupervisorCommand(interviewId, this.authorizedUser.Id))
                    .ConfigureAwait(false);
            }
        }
    }
}
