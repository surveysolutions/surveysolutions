using System;

namespace CAPI.Android.Core.Model.ViewModel.Dashboard
{
    public class DashboardInput
    {
        public DashboardInput(Guid userId)
        {
            UserId = userId;
        }

        public Guid UserId { get; private set; }
    }
}