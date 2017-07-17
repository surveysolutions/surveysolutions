using System.Web.Mvc;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Core.BoundedContexts.Headquarters.WebInterview;
using WB.Core.GenericSubdomains.Portable.ServiceLocation;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.UI.Headquarters.Code;
using WB.UI.Headquarters.Resources;

namespace WB.UI.Headquarters.Filters
{
    public class WebInterviewAuthorizeAttribute : ActionFilterAttribute
    {
        public WebInterviewAuthorizeAttribute()
        {
            this.Order = 30;
        }

        private IWebInterviewConfigProvider webInterviewConfigProvider => ServiceLocator.Current.GetInstance<IWebInterviewConfigProvider>();
        private IQueryableReadSideRepositoryReader<InterviewSummary> InterviewSummaryStorage => 
            ServiceLocator.Current.GetInstance<IQueryableReadSideRepositoryReader<InterviewSummary>>();

        private IAuthorizedUser authorizedUser => ServiceLocator.Current.GetInstance<IAuthorizedUser>();

        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            var interviewId = filterContext.ActionParameters[@"id"].ToString();
            var interviewSummary = this.InterviewSummaryStorage.GetById(interviewId);

            if (interviewSummary != null)
            {
                if (interviewSummary.IsDeleted)
                    throw new WebInterviewAccessException(InterviewAccessExceptionReason.InterviewExpired, WebInterview.Error_InterviewExpired);

                if (interviewSummary.Status != InterviewStatus.InterviewerAssigned
                    && interviewSummary.Status != InterviewStatus.Restarted)
                {
                    throw new WebInterviewAccessException(InterviewAccessExceptionReason.NoActionsNeeded, WebInterview.Error_NoActionsNeeded);
                }

                var webInterviewConfig = this.webInterviewConfigProvider.Get(new QuestionnaireIdentity(interviewSummary.QuestionnaireId, interviewSummary.QuestionnaireVersion));
                if (!webInterviewConfig.Started)
                {
                    if (authorizedUser.IsAuthenticated && authorizedUser.IsInterviewer)
                    {
                        return;
                    }

                    throw new WebInterviewAccessException(InterviewAccessExceptionReason.InterviewExpired, WebInterview.Error_InterviewExpired);
                }
            }
            else
                throw new WebInterviewAccessException(InterviewAccessExceptionReason.InterviewNotFound, WebInterview.Error_NotFound);
        }
    }
}