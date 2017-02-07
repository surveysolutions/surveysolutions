using System.Net;
using System.Web.Mvc;
using Microsoft.Practices.ServiceLocation;
using WB.Core.BoundedContexts.Headquarters.Factories;
using WB.Core.BoundedContexts.Headquarters.WebInterview;
using WB.Core.SharedKernels.DataCollection.Aggregates;
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

        private IStatefullWebInterviewFactory statefulInterviewRepository => ServiceLocator.Current.GetInstance<IStatefullWebInterviewFactory>();

        private IWebInterviewConfigProvider webInterviewConfigProvider => ServiceLocator.Current.GetInstance<IWebInterviewConfigProvider>();

        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            var interviewId = filterContext.ActionParameters[@"id"].ToString();

            IStatefulInterview interview = this.statefulInterviewRepository.Get(interviewId);
            if (interview != null)
            {
                if (interview.Status != InterviewStatus.InterviewerAssigned)
                {
                    throw new WebInterviewAccessException(WebInterviewAccessException.ExceptionReason.NoActionsNeeded,
                        Resources.WebInterview.Error_NoActionsNeeded);
                }

                var webInterviewConfig = this.webInterviewConfigProvider.Get(interview.QuestionnaireIdentity);
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