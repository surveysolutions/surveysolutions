using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Cirrious.MvvmCross.ViewModels;
using WB.Core.BoundedContexts.Interviewer.Properties;
using WB.Core.BoundedContexts.Interviewer.Services;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.SharedKernels.Enumerator.ViewModels;

namespace WB.Core.BoundedContexts.Interviewer.Views
{
    public class BandwidthTestViewModel : BaseViewModel
    {
        private readonly INetworkService networkService;
        private readonly IRestService restService;
        private readonly int pingAttempCount = 5;
        
        private bool isConnectionAbsent;
        private bool isBandwidthTested;
        private string connectionDescription;
        private string connectionType;
        private string networkName;
        private string ping;
        private bool isInProgress;

        public BandwidthTestViewModel(
            INetworkService networkService,
            IRestService restService)
        {
            this.networkService = networkService;
            this.restService = restService;
        }

        public bool IsConnectionAbsent
        {
            get { return this.isConnectionAbsent; }
            set { this.RaiseAndSetIfChanged(ref this.isConnectionAbsent, value); }
        }

        public bool IsBandwidthTested
        {
            get { return this.isBandwidthTested; }
            set { this.RaiseAndSetIfChanged(ref this.isBandwidthTested, value); }
        }

        public bool IsInProgress
        {
            get { return this.isInProgress; }
            set { this.RaiseAndSetIfChanged(ref this.isInProgress, value); }
        }

        public string ConnectionType
        {
            get { return this.connectionType; }
            set { this.RaiseAndSetIfChanged(ref this.connectionType, value); }
        }

        public string ConnectionDescription
        {
            get { return this.connectionDescription; }
            set { this.RaiseAndSetIfChanged(ref this.connectionDescription, value); }
        }

        public string NetworkName
        {
            get { return this.networkName; }
            set { this.RaiseAndSetIfChanged(ref this.networkName, value); }
        }

        public string Ping
        {
            get { return this.ping; }
            set { this.RaiseAndSetIfChanged(ref this.ping, value); }
        }

        public IMvxCommand TestConnectionCommand
        {
            get { return new MvxCommand(async () => await this.TestConnection()); }
        }

        private async Task TestConnection()
        {

            this.IsBandwidthTested = false;
            this.IsConnectionAbsent = false;

            if (!this.networkService.IsNetworkEnabled())
            {
                this.IsBandwidthTested = true;
                this.IsConnectionAbsent = true;
                this.ConnectionDescription = InterviewerUIResources.Diagnostics_BandwidthTestConnectionAbsent_Title;
                return;
            }

            this.IsInProgress = true;

            await Task.Run(async () =>
            {
                var time = new List<double>();
                for (int i = 0; i < pingAttempCount; i++)
                {
                    var beforeTime = DateTime.Now;
                    try
                    {
                        await this.restService.GetAsync(string.Empty);
                    }
                    catch
                    {
                        continue;
                    }
                    var afterTime = DateTime.Now;
                    var timeDifference = afterTime - beforeTime;
                    time.Add(timeDifference.TotalMilliseconds);
                }

                if (time.Any())
                {
                    this.Ping = (int)time.Average() + "ms";
                }

                this.ConnectionType = this.networkService.GetNetworkTypeName();
                this.NetworkName = this.networkService.GetNetworkName();

                this.IsConnectionAbsent = time.Count == 0;
                this.ConnectionDescription = this.IsConnectionAbsent
                    ? this.ConnectionDescription =
                        InterviewerUIResources.Diagnostics_BandwidthTestConnectionToTheServerAbsent_Title
                    : (time.Count == pingAttempCount
                        ? InterviewerUIResources.Diagnostics_BandwidthTestConnectionOK_Title
                        : InterviewerUIResources.Diagnostics_BandwidthTestConnectionNotOK_Title);
            });
            
            this.IsInProgress = false;
            this.IsBandwidthTested = true;
        }
    }
}