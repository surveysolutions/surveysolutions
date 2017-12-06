using System;

namespace WB.UI.Headquarters.API.WebInterview
{
    public interface IReviewAllowedService
    {
        void CheckIfAllowed(Guid interviewId);
    }
}