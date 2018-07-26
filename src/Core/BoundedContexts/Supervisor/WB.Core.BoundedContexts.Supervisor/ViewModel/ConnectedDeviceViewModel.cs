using MvvmCross.Commands;
using MvvmCross.ViewModels;
using WB.Core.SharedKernels.Enumerator.ViewModels;
using WB.Core.SharedKernels.Enumerator.Views;

namespace WB.Core.BoundedContexts.Supervisor.ViewModel
{
    public class ConnectedDeviceViewModel : MvxViewModel
    {
        private string interviewerName;
        private SendingDeviceStatus status;
        private SynchronizationStatistics statistics;

        public ConnectedDeviceViewModel()
        {
            this.Synchronization = new ConnectedDeviceSynchronizationViewModel();
        }

        public string InterviewerName
        {
            get => interviewerName;
            set => SetProperty(ref interviewerName, value);
        }

        public IMvxCommand AbortCommand => new MvxCommand(Abort);

        private void Abort()
        {
        }

        public ConnectedDeviceSynchronizationViewModel Synchronization { get; set; }
    }

    public enum SendingDeviceStatus
    {
        Connected,
        Disconnected,
        Found,
        ConnectionRequested,
        Synchronizing,
        DoneWithErrors,
        Done
    }
}
