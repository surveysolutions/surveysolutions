using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using MvvmCross.Commands;
using MvvmCross.Plugin.WebBrowser;
using MvvmCross.ViewModels;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.SharedKernels.Enumerator.Properties;
using WB.Core.SharedKernels.Enumerator.Services;

namespace WB.Core.SharedKernels.Enumerator.ViewModels
{
    public class BandwidthTestViewModel : MvxNotifyPropertyChanged
    {
        private const int CountOfPingAttemps = 5;

        private readonly INetworkService networkService;
        private readonly IDeviceSettings deviceSettings;
        private readonly IRestService restService;
        private readonly IMvxWebBrowserTask webBrowser;

        private bool isConnectionAbsent;
        private bool isBandwidthTested;
        private string connectionDescription;
        private string connectionType;
        private string networkName;
        private string ping;
        private bool isInProgress;

        public BandwidthTestViewModel(
            INetworkService networkService,
            IDeviceSettings deviceSettings,
            IRestService restService,
            IMvxWebBrowserTask webBrowser)
        {
            this.networkService = networkService;
            this.deviceSettings = deviceSettings;
            this.restService = restService;
            this.webBrowser = webBrowser;
        }

        public bool IsConnectionAbsent
        {
            get => this.isConnectionAbsent;
            set => this.RaiseAndSetIfChanged( ref this.isConnectionAbsent, value);
        }

        public bool IsBandwidthTested
        {
            get => this.isBandwidthTested;
            set => this.RaiseAndSetIfChanged( ref this.isBandwidthTested, value);
        }

        public bool IsInProgress
        {
            get => this.isInProgress;
            set => this.RaiseAndSetIfChanged( ref this.isInProgress, value);
        }

        public string ConnectionType
        {
            get => this.connectionType;
            set => this.RaiseAndSetIfChanged( ref this.connectionType, value);
        }

        public string ConnectionDescription
        {
            get => this.connectionDescription;
            set => this.RaiseAndSetIfChanged( ref this.connectionDescription, value);
        }

        public string NetworkName
        {
            get => this.networkName;
            set => this.RaiseAndSetIfChanged( ref this.networkName, value);
        }

        public string Ping
        {
            get => this.ping;
            set => this.RaiseAndSetIfChanged( ref this.ping, value);
        }

        public string ServerUrl => this.deviceSettings.Endpoint;

        public IMvxAsyncCommand TestConnectionCommand => new MvxAsyncCommand(this.TestConnectionAsync, () => !string.IsNullOrEmpty(this.ServerUrl));

        public IMvxCommand OpenSyncEndPointCommand => new MvxCommand(() =>
        {
            if (Uri.TryCreate(this.deviceSettings.Endpoint, UriKind.Absolute, out Uri _))
            {
                this.webBrowser.ShowWebPage(this.deviceSettings.Endpoint);
            }
            else
            {
                this.IsBandwidthTested = true;
                this.IsConnectionAbsent = true;
                this.ConnectionDescription = InterviewerUIResources.InvalidEndpoint;
            }
        });

        private async Task TestConnectionAsync()
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
            
            var pingsInMilliseconds = new double[CountOfPingAttemps];
            int countOfFailedPingAttemps = 0;
            for (int pingIndex = 0; pingIndex < CountOfPingAttemps; pingIndex++)
            {
                try
                {
                    var stopwatch = new Stopwatch();
                    stopwatch.Start();
                    await this.restService.GetAsync(this.deviceSettings.BandwidthTestUri);
                    stopwatch.Stop();
                    pingsInMilliseconds[pingIndex] = stopwatch.ElapsedMilliseconds;
                }
                catch
                {
                    countOfFailedPingAttemps++;
                }
            }

            if (pingsInMilliseconds.Any())
            {
                this.Ping = Convert.ToInt32(pingsInMilliseconds.Average()) + "ms";
            }

            this.ConnectionType = this.networkService.GetNetworkType();
            this.NetworkName = this.networkService.GetNetworkName();

            this.IsConnectionAbsent = countOfFailedPingAttemps == CountOfPingAttemps;
            this.ConnectionDescription = this.IsConnectionAbsent
                ? this.ConnectionDescription =
                    InterviewerUIResources.Diagnostics_BandwidthTestConnectionToTheServerAbsent_Title
                : (countOfFailedPingAttemps == 0
                    ? InterviewerUIResources.Diagnostics_BandwidthTestConnectionOK_Title
                    : InterviewerUIResources.Diagnostics_BandwidthTestConnectionNotOK_Title);

            this.IsInProgress = false;
            this.IsBandwidthTested = true;
        }
    }
}
