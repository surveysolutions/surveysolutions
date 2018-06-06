using System;

namespace WB.Core.SharedKernels.SurveyManagement.Web.Code
{
    public interface IRevalidateInterviewsAdministrationService
    {
        void RevalidateAllInterviewsWithErrorsAsync(Guid userId, DateTime? startDate, DateTime? endDate);

        string GetReadableStatus();

        void StopInterviewsRevalidating();

    }
}
