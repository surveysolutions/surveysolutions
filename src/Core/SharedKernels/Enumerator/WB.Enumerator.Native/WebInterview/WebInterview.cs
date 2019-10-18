using System;
using System.ComponentModel;
using System.Threading.Tasks;
using Microsoft.AspNet.SignalR;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Exceptions;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Enumerator.Native.WebInterview.Pipeline;

namespace WB.Enumerator.Native.WebInterview
{
    public abstract class WebInterview : Hub
    {
        private readonly IPipelineModule[] hubPipelineModules;

        protected string CallerInterviewId => this.Context.QueryString[@"interviewId"];


        protected WebInterview(IPipelineModule[] hubPipelineModules)
        {
            this.hubPipelineModules = hubPipelineModules;
        }

        public override async Task OnConnected()
        {
            await RegisterClient();

            foreach (var pipelineModule in hubPipelineModules)
            {
                await pipelineModule.OnConnected(this);
            }
            await base.OnConnected();
        }

        public override async Task OnDisconnected(bool stopCalled)
        {
            foreach (var pipelineModule in hubPipelineModules)
            {
                await pipelineModule.OnDisconnected(this, stopCalled);
            }
            
            await UnregisterClient();

            await base.OnDisconnected(stopCalled);
        }

        public override async Task OnReconnected()
        {
            foreach (var pipelineModule in hubPipelineModules)
            {
                await pipelineModule.OnReconnected(this);
            }
            await base.OnReconnected();
        }

        private async Task RegisterClient()
        {
            var interviewId = Context.QueryString[@"interviewId"];
            IStatefulInterview interview = null;

            InScopeExecutor.Current.Execute(ls =>
            {
                var interviewRepository = ls.GetInstance<IStatefulInterviewRepository>();
                interview = interviewRepository.Get(interviewId);
            });

            if (interview == null)
            {
                Clients.Caller.shutDown();
                return;
            }

            var questionnaireId = interview.QuestionnaireIdentity.ToString();

            await Groups.Add(Context.ConnectionId, interviewId);

            var isReview = Context.QueryString[@"review"].ToBool(false);

            if (!isReview)
            {
                Clients.OthersInGroup(interviewId).closeInterview();
            }

            await Groups.Add(Context.ConnectionId, questionnaireId);
        }

        private async Task UnregisterClient()
        {
            var interviewId = Context.QueryString[@"interviewId"];
            var sectionId = Clients.CallerState.sectionId as string;

            await Groups.Remove(Context.ConnectionId, GetGroupNameBySectionIdentity(sectionId, interviewId));
            await Groups.Remove(Context.ConnectionId, interviewId);

            IStatefulInterview interview = null;

            InScopeExecutor.Current.Execute(ls =>
            {
                var interviewRepository = ls.GetInstance<IStatefulInterviewRepository>();
                interview = interviewRepository.Get(interviewId);
            });

            if (interview == null)
                return;

            var questionnaireId = interview.QuestionnaireIdentity.ToString();
            await Groups.Remove(Context.ConnectionId, questionnaireId);
        }

        public async Task ChangeSection(string oldSection)
        {
            var interviewId = Context.QueryString[@"interviewId"];
            var sectionId = Clients.CallerState.sectionId as string;

            if (interviewId != null)
            {
                await Groups.Remove(Context.ConnectionId, GetGroupNameBySectionIdentity(oldSection, interviewId));
                await Groups.Add(Context.ConnectionId, GetGroupNameBySectionIdentity(sectionId, interviewId));
            }
        }

        private static string GetGroupNameBySectionIdentity(string sectionId, string interviewId)
        {
            return sectionId == null
                ? GetConnectedClientPrefilledSectionKey(Guid.Parse(interviewId))
                : GetConnectedClientSectionKey(Identity.Parse(sectionId), Guid.Parse(interviewId));
        }


        [Localizable(false)]
        public static string GetConnectedClientSectionKey(Identity sectionId, Guid interviewId) => $"{sectionId}x{interviewId}";

        [Localizable(false)]
        public static string GetConnectedClientPrefilledSectionKey(Guid interviewId) => $"PrefilledSectionx{interviewId}";
        

        public static string GetUiMessageFromException(Exception e)
        {
            if (e is InterviewException interviewException && interviewException.ExceptionType != InterviewDomainExceptionType.Undefined)
            {
                switch (interviewException.ExceptionType)
                {
                    case InterviewDomainExceptionType.InterviewLimitReached:
                        return Enumerator.Native.Resources.WebInterview.ServerUnderLoad;
                    case InterviewDomainExceptionType.QuestionnaireIsMissing:
                    case InterviewDomainExceptionType.InterviewHardDeleted:
                        return Enumerator.Native.Resources.WebInterview.Error_InterviewExpired;
                    case InterviewDomainExceptionType.OtherUserIsResponsible:
                    case InterviewDomainExceptionType.StatusIsNotOneOfExpected:
                        return Enumerator.Native.Resources.WebInterview.Error_NoActionsNeeded;
                    case InterviewDomainExceptionType.InterviewRecievedByDevice:
                        return Enumerator.Native.Resources.WebInterview.InterviewReceivedByInterviewer;
                    case InterviewDomainExceptionType.InterviewSizeLimitReached:
                        return Enumerator.Native.Resources.WebInterview.InterviewSizeLimitReached;
                }
            }

            return e.Message;
        }
        
        public void Ping()
        {
            //
        }
    }
}
