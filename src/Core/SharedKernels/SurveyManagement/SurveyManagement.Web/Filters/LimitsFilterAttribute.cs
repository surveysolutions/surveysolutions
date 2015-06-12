
using System.Web.Mvc;
using Microsoft.Practices.ServiceLocation;
using WB.Core.Infrastructure.Transactions;
using WB.Core.SharedKernels.DataCollection.Services;

namespace WB.Core.SharedKernels.SurveyManagement.Web.Filters
{
    public class LimitsFilterAttribute : ActionFilterAttribute
    {
        private IInterviewPreconditionsService InterviewPreconditionsService
        {
            get { return ServiceLocator.Current.GetInstance<IInterviewPreconditionsService>(); }
        }

        private ITransactionManagerProvider TransactionManagerProvider
        {
            get { return ServiceLocator.Current.GetInstance<ITransactionManagerProvider>(); }
        }

        public override void OnActionExecuted(ActionExecutedContext filterContext)
        {
            var viewResult = filterContext.Result as ViewResult;
            if (viewResult != null)
            {
                var shouldUseOwnTransaction = !TransactionManagerProvider.GetTransactionManager().IsQueryTransactionStarted;

                if (shouldUseOwnTransaction)
                {
                    this.TransactionManagerProvider.GetTransactionManager().BeginQueryTransaction();
                }
                try
                {
                    var mxAllowedInterviewsCount = InterviewPreconditionsService.GetMaxAllowedInterviewsCount();

                    viewResult.ViewBag.ShowInterviewLimitIndicator = mxAllowedInterviewsCount.HasValue;

                    if (mxAllowedInterviewsCount.HasValue)
                    {
                        var limit = mxAllowedInterviewsCount.Value;
                        var interviewsLeft =
                            InterviewPreconditionsService.GetInterviewsCountAllowedToCreateUntilLimitReached();
                        viewResult.ViewBag.InterviewsCountAllowedToCreateUntilLimitReached = interviewsLeft;
                        viewResult.ViewBag.PayAttentionOnInterviewLimitIndicator = interviewsLeft <= (limit/10);
                    }
                }
                finally
                {
                    if (shouldUseOwnTransaction)
                    {
                        this.TransactionManagerProvider.GetTransactionManager().RollbackQueryTransaction();
                    }
                }
            }
            base.OnActionExecuted(filterContext);
        }
    }
}
