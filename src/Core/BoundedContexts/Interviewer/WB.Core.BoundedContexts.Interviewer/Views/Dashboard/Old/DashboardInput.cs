using System;

namespace WB.UI.Interviewer.ViewModel.Dashboard
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