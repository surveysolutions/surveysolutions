using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using MvvmCross.Commands;
using MvvmCross.ViewModels;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.HttpServices.Services;
using WB.Core.SharedKernels.Enumerator.Properties;
using WB.Core.SharedKernels.Enumerator.Services;
using Xamarin.Essentials;

namespace WB.Core.SharedKernels.Enumerator.ViewModels
{
    public class BandwidthTestViewModel : MvxNotifyPropertyChanged
    {
        private const int CountOfPingAttempts = 5;

        private readonly INetworkService networkService;
        private readonly IDeviceSettings deviceSettings;
        private readonly IRestService restService;

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
            IRestService restService)
        {
            this.networkService = networkService;
            this.deviceSettings = deviceSettings;
            this.restService = restService;

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

        public IMvxCommand OpenSyncEndPointCommand => new MvxAsyncCommand(async () =>
        {
            if (Uri.TryCreate(this.deviceSettings.Endpoint, UriKind.Absolute, out Uri _))
            {
                try
                {
                    await Browser.OpenAsync(this.deviceSettings.Endpoint, BrowserLaunchMode.SystemPreferred);
                }
                catch(Exception ex)
                {
                    // An unexpected error occured. No browser may be installed on the device.
                    this.ConnectionDescription = ex.Message;
                }
                //this.webBrowser.ShowWebPage(this.deviceSettings.Endpoint);
            }
            else
            {
                this.IsBandwidthTested = true;
                this.IsConnectionAbsent = true;
                this.ConnectionDescription = EnumeratorUIResources.InvalidEndpoint;
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
                this.ConnectionDescription = EnumeratorUIResources.Diagnostics_BandwidthTestConnectionAbsent_Title;
                return;
            }

            this.IsInProgress = true;
            
            var pingsInMilliseconds = new double[CountOfPingAttempts];
            int countOfFailedPingAttemps = 0;
            for (int pingIndex = 0; pingIndex < CountOfPingAttempts; pingIndex++)
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

            this.IsConnectionAbsent = countOfFailedPingAttemps == CountOfPingAttempts;
            this.ConnectionDescription = this.IsConnectionAbsent
                ? this.ConnectionDescription =
                    EnumeratorUIResources.Diagnostics_BandwidthTestConnectionToTheServerAbsent_Title
                : (countOfFailedPingAttemps == 0
                    ? EnumeratorUIResources.Diagnostics_BandwidthTestConnectionOK_Title
                    : EnumeratorUIResources.Diagnostics_BandwidthTestConnectionNotOK_Title);

            this.IsInProgress = false;
            this.IsBandwidthTested = true;
        }
    }
}
