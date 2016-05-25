
using System.Linq;
using System.Web.Mvc;
using Microsoft.Practices.ServiceLocation;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.Infrastructure.Transactions;
using WB.Core.SharedKernels.SurveyManagement.Implementation.Services;
using WB.Core.SharedKernels.SurveyManagement.Views.Interview;

namespace WB.Core.SharedKernels.SurveyManagement.Web.Filters
{
    public class LimitsFilterAttribute : ActionFilterAttribute
    {
        private InterviewPreconditionsServiceSettings InterviewPreconditionsServiceSettings
        {
            get { return ServiceLocator.Current.GetInstance<InterviewPreconditionsServiceSettings>(); }
        }

        private ITransactionManagerProvider TransactionManagerProvider
        {
            get { return ServiceLocator.Current.GetInstance<ITransactionManagerProvider>(); }
        }

        private IQueryableReadSideRepositoryReader<InterviewSummary> InterviewSummaryStorage
        {
            get { return ServiceLocator.Current.GetInstance<IQueryableReadSideRepositoryReader<InterviewSummary>>(); }
        }

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
                    viewResult.ViewBag.PayAttentionOnInterviewLimitIndicator = interviewsLeft <= (limit/10);
                }

            }
            base.OnActionExecuted(filterContext);
        }

        private int QueryInterviewsCount()
        {
            var shouldUseOwnTransaction = !TransactionManagerProvider.GetTransactionManager().IsQueryTransactionStarted;

            if (shouldUseOwnTransaction)
            {
                this.TransactionManagerProvider.GetTransactionManager().BeginQueryTransaction();
            }
            try
            {
                return InterviewSummaryStorage.Query(_ => _.Select(i => i.InterviewId).Count());
            }
            finally
            {
                if (shouldUseOwnTransaction)
                {
                    this.TransactionManagerProvider.GetTransactionManager().RollbackQueryTransaction();
                }
            }
        }
    }
}
