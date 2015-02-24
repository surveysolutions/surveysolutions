using System;

namespace WB.UI.Capi.ViewModel.Dashboard
{
    public class DashboardInput
    {
        public DashboardInput(Guid userId)
        {
            this.UserId = userId;
        }

        public Guid UserId { get; private set; }
    }
}