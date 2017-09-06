using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using MvvmCross.Core.ViewModels;
using MvvmCross.Plugins.WebBrowser;
using WB.Core.BoundedContexts.Interviewer.Properties;
using WB.Core.BoundedContexts.Interviewer.Services;
using WB.Core.GenericSubdomains.Portable.Services;

namespace WB.Core.BoundedContexts.Interviewer.Views
{
    public class BandwidthTestViewModel : MvxNotifyPropertyChanged
    {
        private readonly INetworkService networkService;
        private readonly IInterviewerSettings interviewerSettings;
        private readonly IRestService restService;
        private readonly IMvxWebBrowserTask webBrowser;
        private const int countOfPingAttemps = 5;

        private bool isConnectionAbsent;
        private bool isBandwidthTested;
        private string connectionDescription;
        private string connectionType;
        private string networkName;
        private string ping;
        private bool isInProgress;

        public BandwidthTestViewModel(
            INetworkService networkService,
            IInterviewerSettings interviewerSettings,
            IRestService restService,
            IMvxWebBrowserTask webBrowser)
        {
            this.networkService = networkService;
            this.interviewerSettings = interviewerSettings;
            this.restService = restService;
            this.webBrowser = webBrowser;
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

        public string ServerUrl => this.interviewerSettings.Endpoint;

        public IMvxAsyncCommand TestConnectionCommand => new MvxAsyncCommand(this.TestConnectionAsync, () => !string.IsNullOrEmpty(this.ServerUrl));

        public IMvxCommand OpenSyncEndPointCommand => new MvxCommand(() =>
        {
            if (Uri.TryCreate(this.interviewerSettings.Endpoint, UriKind.Absolute, out Uri _))
            {
                this.webBrowser.ShowWebPage(this.interviewerSettings.Endpoint);
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
            
            var pingsInMilliseconds = new double[countOfPingAttemps];
            int countOfFailedPingAttemps = 0;
            for (int pingIndex = 0; pingIndex < countOfPingAttemps; pingIndex++)
            {
                try
                {
                    var stopwatch = new Stopwatch();
                    stopwatch.Start();
                    await this.restService.GetAsync(this.interviewerSettings.BandwidthTestUri);
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

            this.IsConnectionAbsent = countOfFailedPingAttemps == countOfPingAttemps;
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