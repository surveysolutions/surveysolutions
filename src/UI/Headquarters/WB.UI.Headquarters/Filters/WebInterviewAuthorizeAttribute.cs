using System.Web.Mvc;
using Microsoft.Practices.ServiceLocation;
using WB.Core.BoundedContexts.Headquarters.Factories;
using WB.Core.BoundedContexts.Headquarters.WebInterview;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;

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

            var interview = this.statefulInterviewRepository.Get(interviewId);
            if (interview != null)
            {
                if (interview.Status != InterviewStatus.InterviewerAssigned)
                {
                    filterContext.Result = new HttpNotFoundResult();
                }

                var webInterviewConfig = this.webInterviewConfigProvider.Get(interview.QuestionnaireIdentity);
                if (!webInterviewConfig.Started)
                {
                    filterContext.Result = new HttpNotFoundResult();
                }
            }
            else filterContext.Result = new HttpNotFoundResult();
        }
    }
}