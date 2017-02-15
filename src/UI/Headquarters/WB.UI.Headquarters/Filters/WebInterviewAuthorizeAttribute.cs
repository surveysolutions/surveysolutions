using System.Net;
using System.Web.Mvc;
using Microsoft.Practices.ServiceLocation;
using WB.Core.BoundedContexts.Headquarters.Factories;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Core.BoundedContexts.Headquarters.WebInterview;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.UI.Headquarters.Code;

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

        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            var interviewId = filterContext.ActionParameters[@"id"].ToString();
            var interviewSummary = this.InterviewSummaryStorage.GetById(interviewId);

            if (interviewSummary != null)
            {
                if (interviewSummary.IsDeleted)
                    throw new WebInterviewAccessException(WebInterviewAccessException.ExceptionReason.InterviewExpired,
                        Resources.WebInterview.Error_InterviewExpired);

                if (interviewSummary.Status != InterviewStatus.InterviewerAssigned)
                {
                    throw new WebInterviewAccessException(WebInterviewAccessException.ExceptionReason.NoActionsNeeded,
                        Resources.WebInterview.Error_NoActionsNeeded);
                }

                var webInterviewConfig = this.webInterviewConfigProvider.Get(new QuestionnaireIdentity(interviewSummary.QuestionnaireId, interviewSummary.QuestionnaireVersion));
                if (!webInterviewConfig.Started)
                {
                    throw new WebInterviewAccessException(WebInterviewAccessException.ExceptionReason.InterviewExpired,
                        Resources.WebInterview.Error_InterviewExpired);
                }
            }
            else
                throw new WebInterviewAccessException(WebInterviewAccessException.ExceptionReason.InterviewNotFound,
                    Resources.WebInterview.Error_NotFound);
        }
    }
}