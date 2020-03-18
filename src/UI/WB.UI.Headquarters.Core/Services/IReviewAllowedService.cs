using System;

namespace WB.UI.Headquarters.Services
{
    public interface IReviewAllowedService
    {
        void CheckIfAllowed(Guid interviewId);
    }
}
