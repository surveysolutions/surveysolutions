using MvvmCross.Commands;
using MvvmCross.ViewModels;
using WB.Core.SharedKernels.Enumerator.Views;

namespace WB.Core.BoundedContexts.Supervisor.ViewModel
{
    public class ConnectedDeviceViewModel : MvxViewModel
    {
        private string interviewerName;
        private SendingDeviceStatus status;
        private SynchronizationStatistics statistics;

        public string InterviewerName
        {
            get => interviewerName;
            set => SetProperty(ref interviewerName, value);
        }

        public SendingDeviceStatus Status
        {
            get => status;
            set => SetProperty(ref status, value);
        }

        //public override Task Initialize()
        //{
        //    this.ProgressStatus = string.Format(SupervisorUIResources.OfflineSync_TransferInterviewsFormat, "interviewer1", 2, 5);

        //    var speed = ByteSize.FromKilobytes(352);
        //    var measurementInterval = TimeSpan.FromSeconds(1);
        //    var kbPerSec = speed.Per(measurementInterval).Humanize();

        //    var minLeft = TimeSpan.FromMinutes(10).Humanize();

        //    this.ProgressInPercents = 33;
        //    this.TransferingStatus = TransferingStatus.Transferring;

        //    return base.Initialize();
        //}

        public IMvxCommand AbortCommand => new MvxCommand(Abort);

        public SynchronizationStatistics Statistics
        {
            get => statistics;
            set => SetProperty(ref statistics, value);
        }

        private void Abort()
        {
        }
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
