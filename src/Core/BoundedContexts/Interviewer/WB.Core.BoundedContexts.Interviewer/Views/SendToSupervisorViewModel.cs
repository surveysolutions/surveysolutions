using System;
using System.Threading.Tasks;
using Humanizer;
using Humanizer.Bytes;
using MvvmCross.Commands;
using WB.Core.SharedKernels.Enumerator.Properties;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;
using WB.Core.SharedKernels.Enumerator.ViewModels;
using WB.Core.SharedKernels.Enumerator.Views;
using InterviewerUIResources = WB.Core.BoundedContexts.Interviewer.Properties.InterviewerUIResources;

namespace WB.Core.BoundedContexts.Interviewer.Views
{
    public class SendToSupervisorViewModel : BaseViewModel
    {
        public SendToSupervisorViewModel(IPrincipal principal, 
            IViewModelNavigationService viewModelNavigationService) : base(principal, viewModelNavigationService)
        {
        }

        private string title;
        public string Title
        {
            get => this.title;
            set => this.SetProperty(ref this.title, value);
        }

        private string progressTitle;
        public string ProgressTitle
        {
            get => this.progressTitle;
            set => this.SetProperty(ref this.progressTitle, value);
        }

        private bool showNote;
        public bool ShowNote
        {
            get => this.showNote;
            set => this.SetProperty(ref this.showNote, value);
        }

        private string progressStatus;
        public string ProgressStatus
        {
            get => this.progressStatus;
            set => this.SetProperty(ref this.progressStatus, value);
        }

        private string networkInfo;
        public string NetworkInfo
        {
            get => this.networkInfo;
            set => this.SetProperty(ref this.networkInfo, value);
        }

        private int progressInPercents;
        public int ProgressInPercents
        {
            get => this.progressInPercents;
            set => this.SetProperty(ref this.progressInPercents, value);
        }

        private TransferingStatus transferingStatus;
        public TransferingStatus TransferingStatus
        {
            get => this.transferingStatus;
            set => this.SetProperty(ref this.transferingStatus, value);
        }

        public override Task Initialize()
        {
            this.Title = InterviewerUIResources.SendToSupervisor_MovingToSupervisorDevice;
            this.ProgressTitle = InterviewerUIResources.SendToSupervisor_CheckSupervisorDevice;

            this.ProgressStatus = string.Format(InterviewerUIResources.SendToSupervisor_TransferedInterviews, 2, 5);

            this.ShowNote = true;

            var speed = ByteSize.FromKilobytes(352);
            var measurementInterval = TimeSpan.FromSeconds(1);
            var kbPerSec = speed.Per(measurementInterval).Humanize();

            var minLeft = TimeSpan.FromMinutes(10).Humanize();

            this.NetworkInfo = string.Format(UIResources.OfflineSync_NetworkInfo, kbPerSec, minLeft);
            this.ProgressInPercents = 33;
            this.TransferingStatus = TransferingStatus.CompletedWithErrors;

            return base.Initialize();
        }

        public IMvxCommand CancelCommand => new MvxCommand(Done);
        public IMvxCommand AbortCommand => new MvxCommand(Done);
        public IMvxCommand RetryCommand => new MvxCommand(Retry);
        public IMvxCommand DoneCommand => new MvxCommand(Done);

        private void Done() => this.viewModelNavigationService.NavigateToDashboardAsync();

        private void Retry()
        {
            throw new NotImplementedException();
        }
    }
}
