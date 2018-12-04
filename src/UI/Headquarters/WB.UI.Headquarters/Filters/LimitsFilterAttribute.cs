using System.Linq;
using System.Web.Mvc;
using WB.Core.BoundedContexts.Headquarters.Implementation.Services;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;

namespace WB.Core.SharedKernels.SurveyManagement.Web.Filters
{
    public class LimitsFilterAttribute : ActionFilterAttribute
    {
        private InterviewPreconditionsServiceSettings InterviewPreconditionsServiceSettings =>
            DependencyResolver.Current.GetService<InterviewPreconditionsServiceSettings>();

        private IQueryableReadSideRepositoryReader<InterviewSummary> InterviewSummaryStorage =>
            DependencyResolver.Current.GetService<IQueryableReadSideRepositoryReader<InterviewSummary>>();

        public override void OnActionExecuted(ActionExecutedContext filterContext)
        {
            var viewResult = filterContext.Result as ViewResult;
            if (viewResult != null)
            {
                var maxAllowedInterviewsCount = InterviewPreconditionsServiceSettings.InterviewLimitCount;

                viewResult.ViewBag.ShowInterviewLimitIndicator = maxAllowedInterviewsCount.HasValue;

                if (maxAllowedInterviewsCount.HasValue)
                {
                    var limit = maxAllowedInterviewsCount.Value;
                    var interviewsLeft = InterviewPreconditionsServiceSettings.InterviewLimitCount -
                                         QueryInterviewsCount();

                    viewResult.ViewBag.InterviewsCountAllowedToCreateUntilLimitReached = interviewsLeft;
                    viewResult.ViewBag.MaxAllowedInterviewsCount = maxAllowedInterviewsCount;
                    viewResult.ViewBag.PayAttentionOnInterviewLimitIndicator = interviewsLeft <= (limit / 10);
                }

            }
            base.OnActionExecuted(filterContext);
        }

        private int QueryInterviewsCount()
        {
            return InterviewSummaryStorage.Query(_ => _.Select(i => i.InterviewId).Count());
        }
    }
}
