
using System.Web.Mvc;
using Microsoft.Practices.ServiceLocation;
using WB.Core.SharedKernels.DataCollection.Services;

namespace WB.Core.SharedKernels.SurveyManagement.Web.Filters
{
    public class LimitsFilterAttribute : ActionFilterAttribute
    {
        private readonly IInterviewPreconditionsService interviewPreconditionsService;

        public LimitsFilterAttribute()
        {
            this.interviewPreconditionsService = ServiceLocator.Current.GetInstance<IInterviewPreconditionsService>(); ;
        }

        public override void OnActionExecuted(ActionExecutedContext filterContext)
        {
            var viewResult = filterContext.Result as ViewResult;
            if (viewResult != null)
            {
                var mxAllowedInterviewsCount = interviewPreconditionsService.GetMaxAllowedInterviewsCount();

                viewResult.ViewBag.ShowLimitIndicator = mxAllowedInterviewsCount.HasValue;

                if (mxAllowedInterviewsCount.HasValue)
                {
                    var limit = mxAllowedInterviewsCount.Value;
                    var interviewsLeft = interviewPreconditionsService.GetInterviewsCountAllowedToCreateUntilLimitReached();
                    viewResult.ViewBag.InterviewsLeft = interviewsLeft;
                    viewResult.ViewBag.PayAttention = interviewsLeft <= (limit / 10);
                }
            }
            base.OnActionExecuted(filterContext);
        }
    }
}
