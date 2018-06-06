using System;

namespace WB.Core.SharedKernels.SurveyManagement.Web.Code
{
    public interface IRevalidateInterviewsAdministrationService
    {
        void RevalidateAllInterviewsWithErrorsAsync(Guid userId, DateTime? fromDate, DateTime? toDate);

        string GetReadableStatus();

        void StopInterviewsRevalidating();

    }
}
