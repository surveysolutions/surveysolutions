using System;
using System.Threading.Tasks;
using Humanizer;
using Humanizer.Bytes;
using MvvmCross.Commands;
using MvvmCross.ViewModels;
using WB.Core.BoundedContexts.Supervisor.Properties;
using WB.Core.SharedKernels.Enumerator.Properties;
using WB.Core.SharedKernels.Enumerator.Views;

namespace WB.Core.BoundedContexts.Supervisor.ViewModel
{
    public class ConnectedDeviceViewModel : MvxViewModel
    {

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
            this.ProgressStatus = string.Format(SupervisorUIResources.OfflineSync_TransferInterviewsFormat, "interviewer1", 2, 5);
            
            var speed = ByteSize.FromKilobytes(352);
            var measurementInterval = TimeSpan.FromSeconds(1);
            var kbPerSec = speed.Per(measurementInterval).Humanize();

            var minLeft = TimeSpan.FromMinutes(10).Humanize();

            this.NetworkInfo = string.Format(UIResources.OfflineSync_NetworkInfo, kbPerSec, minLeft);
            this.ProgressInPercents = 33;
            this.TransferingStatus = TransferingStatus.Transferring;

            return base.Initialize();
        }

        public IMvxCommand AbortCommand => new MvxCommand(Abort);

        private void Abort()
        {
            throw new NotImplementedException();
        }
    }
}
