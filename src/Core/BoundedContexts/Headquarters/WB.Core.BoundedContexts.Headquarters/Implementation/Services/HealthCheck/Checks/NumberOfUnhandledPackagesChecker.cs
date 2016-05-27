using System;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.BoundedContexts.Headquarters.ValueObjects.HealthCheck;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.Infrastructure.Transactions;

namespace WB.Core.BoundedContexts.Headquarters.Implementation.Services.HealthCheck.Checks
{
    class NumberOfUnhandledPackagesChecker : IAtomicHealthCheck<NumberOfUnhandledPackagesHealthCheckResult>
    {
        private readonly IInterviewPackagesService interviewPackagesService;
        private readonly IPlainTransactionManager transactionManager;

        public NumberOfUnhandledPackagesChecker(IInterviewPackagesService interviewPackagesService, IPlainTransactionManager transactionManager)
        {
            this.interviewPackagesService = interviewPackagesService;
            this.transactionManager = transactionManager;
        }

        public NumberOfUnhandledPackagesHealthCheckResult Check()
        {
            try
            {
                int count = this.transactionManager.ExecuteInPlainTransaction(
                    () => this.interviewPackagesService.InvalidPackagesCount);

                if (count == 0)
                    return NumberOfUnhandledPackagesHealthCheckResult.Happy(count);

                return NumberOfUnhandledPackagesHealthCheckResult.Warning(count,
                    "The error occurred during processing of the interview.<br />Please, contact Survey Solutions Team <a href='mailto:support@mysurvey.solutions'>support@mysurvey.solutions</a> to inform about the issue.");
            }
            catch (Exception e)
            {
                return NumberOfUnhandledPackagesHealthCheckResult.Error("The information about unhandled packages can't be collected. " + e.Message);
            }
        }
    }
}