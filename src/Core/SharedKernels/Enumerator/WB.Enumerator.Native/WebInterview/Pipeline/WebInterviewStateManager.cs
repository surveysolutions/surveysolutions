using System;
using System.Threading.Tasks;
using Microsoft.AspNet.SignalR.Hubs;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.Versions;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Repositories;

namespace WB.Enumerator.Native.WebInterview.Pipeline
{
    public class WebInterviewStateManager2 : IPipelineModule
    {
        public void OnAfterIncoming(object result, IHubIncomingInvokerContext context)
        {
            var interviewId = context.Hub.Context.QueryString[@"interviewId"];
            var sectionId = context.Hub.Clients.CallerState.sectionId as string;

            if (interviewId != null)
            {
                context.Hub.Groups.Add(context.Hub.Context.ConnectionId,
                    sectionId == null
                        ? WebInterview.GetConnectedClientPrefilledSectionKey(Guid.Parse(interviewId))
                        : WebInterview.GetConnectedClientSectionKey(Identity.Parse(sectionId), Guid.Parse(interviewId)));

                context.Hub.Groups.Add(context.Hub.Context.ConnectionId, interviewId);
            }
        }

        public Task OnConnected(IHub hub)
        {
            var interviewId = hub.Context.QueryString[@"interviewId"];
            string questionnaireId = null;

            InScopeExecutor.Current.Execute(ls =>
            {
                var interviewRepository = ls.GetInstance<IStatefulInterviewRepository>();

                IStatefulInterview interview = interviewRepository.Get(interviewId);

                if (interview == null)
                {
                    hub.Clients.Caller.shutDown();
                    return;
                }

                questionnaireId = interview.QuestionnaireIdentity.ToString();

                var isReview = hub.Context.QueryString[@"review"].ToBool(false);

                if (!isReview)
                {
                    hub.Clients.OthersInGroup(interviewId).closeInterview();
                }

                hub.Groups.Add(hub.Context.ConnectionId, questionnaireId);
            });
            return Task.CompletedTask;
        }

        public Task OnDisconnected(IHub hub, bool stopCalled)
        {
            return Task.CompletedTask;
        }

        public Task OnReconnected(IHub hub)
        {
            return Task.CompletedTask;
        }
    }
}
