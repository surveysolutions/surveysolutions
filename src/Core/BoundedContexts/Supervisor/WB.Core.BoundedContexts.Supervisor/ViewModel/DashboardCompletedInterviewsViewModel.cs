using System.Threading.Tasks;
using MvvmCross.ViewModels;

namespace WB.Core.BoundedContexts.Supervisor.ViewModel
{
    public class DashboardCompletedInterviewsViewModel : MvxViewModel
    {
        public override Task Initialize()
        {
            return Task.CompletedTask;
        }
    }
}
