using System;
using System.Threading;
using System.Threading.Tasks;
using WB.Core.GenericSubdomains.ErrorReporting.Resources;
using WB.Core.GenericSubdomains.ErrorReporting.Services;
using WB.Core.GenericSubdomains.ErrorReporting.Services.CapiInformationService;
using WB.Core.GenericSubdomains.ErrorReporting.Services.TabletInformationSender;
using WB.Core.GenericSubdomains.Utils.Services;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.SharedKernel.Structures.TabletInformation;

namespace WB.Core.GenericSubdomains.ErrorReporting.Implementation.TabletInformation
{
    internal class TabletInformationSender : ITabletInformationSender
    {
        private CancellationToken ct;
        private CancellationTokenSource tokenSource2;
        private Task task;
        private string pathToInfoArchive = null;

        private readonly IFileSystemAccessor fileSystemAccessor;
        private readonly ICapiInformationService capiInformationService;
        private readonly IRestService restService;
        private readonly IErrorReportingSettings errorReportingSettings;
        private readonly ILogger logger;
        
        public TabletInformationSender(ICapiInformationService capiInformationService,
            IFileSystemAccessor fileSystemAccessor, IRestService restService, IErrorReportingSettings errorReportingSettings, ILogger logger)
        {
            this.capiInformationService = capiInformationService;
            this.fileSystemAccessor = fileSystemAccessor;

            this.restService = restService;
            this.errorReportingSettings = errorReportingSettings;
            this.logger = logger;
        }

        public event EventHandler<InformationPackageEventArgs> InformationPackageCreated;
        public event EventHandler<InformationPackageCancellationEventArgs> ProcessCanceled;
        public event EventHandler ProcessFinished;

        public void Run()
        {
            this.tokenSource2 = new CancellationTokenSource();
            this.ct = this.tokenSource2.Token;
            this.task = Task.Factory.StartNew(this.RunInternal, this.ct);
        }

        public void Cancel()
        {
            Cancel(TabletInformationSenderStrings.CanceledByUser);
        }

        private void Cancel(string reason)
        {
            if (this.tokenSource2.IsCancellationRequested)
                return;
            Task.Factory.StartNew(() => this.CancelInternal(reason), ct);
        }

        private async void RunInternal()
        {
            try
            {
                var clientRegistrationId = this.errorReportingSettings.GetClientRegistrationId();

                this.pathToInfoArchive = this.capiInformationService.CreateInformationPackage();

                if (string.IsNullOrEmpty(this.pathToInfoArchive) ||
                    !this.fileSystemAccessor.IsFileExists(this.pathToInfoArchive))
                {
                    this.OnProcessFinished();
                    return;
                }

                this.OnInformationPackageCreated(this.pathToInfoArchive,
                    this.fileSystemAccessor.GetFileSize(this.pathToInfoArchive));

                this.ExitIfCanceled();

                await this.restService.PostAsync(
                    url: "api/InterviewerSync/PostInfoPackage",
                    request: new TabletInformationPackage()
                    {
                        Content = Convert.ToBase64String(this.fileSystemAccessor.ReadAllBytes(this.pathToInfoArchive)),
                        AndroidId = this.errorReportingSettings.GetDeviceId(),
                        ClientRegistrationId = clientRegistrationId
                    });

            }
            catch (Exception e)
            {
                this.logger.Error("Error occurred during the process. Process is being canceled.", e);
                this.Cancel(e.Message);
                return;
            }
            finally
            {
                this.DeleteInfoPackageIfExists();
            }

            this.OnProcessFinished();
        }

        private void OnProcessCanceled(string reason)
        {
            var handler = this.ProcessCanceled;
            if (handler != null)
                handler(this, new InformationPackageCancellationEventArgs(reason));
        }

        private void OnInformationPackageCreated(string filePath, long fileSize)
        {
            if (this.tokenSource2.IsCancellationRequested)
                return;
            var handler = this.InformationPackageCreated;
            if (handler != null)
                handler(this, new InformationPackageEventArgs(filePath, fileSize));
        }

        private void OnProcessFinished()
        {
            if (this.tokenSource2.IsCancellationRequested)
                return;
            var handler = this.ProcessFinished;
            if (handler != null)
                handler(this, EventArgs.Empty);
        }

        private void ExitIfCanceled()
        {
            if (this.ct.IsCancellationRequested)
                this.ct.ThrowIfCancellationRequested();
        }

        private void CancelInternal(string reason)
        {
            this.tokenSource2.Cancel();

            try
            {
                Task.WaitAll(this.task);
            }
            catch (AggregateException e)
            {
                foreach (var exception in e.InnerExceptions)
                {
                    this.logger.Error("Error occurred during the process. Process is being canceled.", exception);
                }
            }
            this.OnProcessCanceled(reason);
            this.DeleteInfoPackageIfExists();
        }

        private void DeleteInfoPackageIfExists()
        {
            if (this.fileSystemAccessor.IsFileExists(this.pathToInfoArchive))
                this.fileSystemAccessor.DeleteFile(this.pathToInfoArchive);
        }
    }
}