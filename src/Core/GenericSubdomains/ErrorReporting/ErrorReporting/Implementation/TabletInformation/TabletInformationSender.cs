using System;
using System.Threading;
using System.Threading.Tasks;
using WB.Core.GenericSubdomains.ErrorReporting.Resources;
using WB.Core.GenericSubdomains.ErrorReporting.Services;
using WB.Core.GenericSubdomains.ErrorReporting.Services.CapiInformationService;
using WB.Core.GenericSubdomains.ErrorReporting.Services.TabletInformationSender;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.SharedKernel.Structures.TabletInformation;

namespace WB.Core.GenericSubdomains.ErrorReporting.Implementation.TabletInformation
{
    internal class TabletInformationSender : ITabletInformationSender
    {
        private CancellationTokenSource tokenSource;

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

        public Task Run()
        {
            this.tokenSource = new CancellationTokenSource();

            return Task.Run(async () =>
            {
                string pathToInfoArchive = null;
                try
                {
                    var clientRegistrationId = this.errorReportingSettings.GetClientRegistrationId();

                    pathToInfoArchive = await this.capiInformationService.CreateInformationPackage(this.tokenSource.Token);

                    if (string.IsNullOrEmpty(pathToInfoArchive) ||
                        !this.fileSystemAccessor.IsFileExists(pathToInfoArchive))
                    {
                        this.OnProcessFinished();
                        return;
                    }

                    this.ExitIfCanceled();

                    this.OnInformationPackageCreated(pathToInfoArchive,
                        this.fileSystemAccessor.GetFileSize(pathToInfoArchive));

                    this.ExitIfCanceled();

                    await this.restService.PostAsync(
                        url: "api/InterviewerSync/PostInfoPackage",
                        request: new TabletInformationPackage()
                        {
                            Content =
                                Convert.ToBase64String(this.fileSystemAccessor.ReadAllBytes(pathToInfoArchive)),
                            AndroidId = this.errorReportingSettings.GetDeviceId(),
                            ClientRegistrationId = clientRegistrationId
                        });

                }
                catch (OperationCanceledException)
                {
                    this.OnProcessCanceled(TabletInformationSenderStrings.CanceledByUser);
                }
                catch (Exception e)
                {
                    this.logger.Error("Error occurred during the process. Process is being canceled.", e);
                    this.OnProcessCanceled(e.Message);
                    return;
                }
                finally
                {
                    this.DeleteInfoPackageIfExists(pathToInfoArchive);
                }

                this.OnProcessFinished();
            }, this.tokenSource.Token);
        }

        public void Cancel()
        {
            this.tokenSource.Cancel();
        }

        private void OnProcessCanceled(string reason)
        {
            var handler = this.ProcessCanceled;
            if (handler != null)
                handler(this, new InformationPackageCancellationEventArgs(reason));
        }

        private void OnInformationPackageCreated(string filePath, long fileSize)
        {
            if (this.tokenSource.IsCancellationRequested)
                return;
            var handler = this.InformationPackageCreated;
            if (handler != null)
                handler(this, new InformationPackageEventArgs(filePath, fileSize));
        }

        private void OnProcessFinished()
        {
            if (this.tokenSource.IsCancellationRequested)
                return;
            var handler = this.ProcessFinished;
            if (handler != null)
                handler(this, EventArgs.Empty);
        }

        private void ExitIfCanceled()
        {
            if (this.tokenSource.Token.IsCancellationRequested)
                this.tokenSource.Token.ThrowIfCancellationRequested();
        }

        private void DeleteInfoPackageIfExists(string pathToInfoArchive)
        {
            if (this.fileSystemAccessor.IsFileExists(pathToInfoArchive))
                this.fileSystemAccessor.DeleteFile(pathToInfoArchive);
        }
    }
}