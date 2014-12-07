using System;
using System.Threading;
using System.Threading.Tasks;
using WB.Core.GenericSubdomains.ErrorReporting.Services;
using WB.Core.GenericSubdomains.ErrorReporting.Services.CapiInformationService;
using WB.Core.GenericSubdomains.ErrorReporting.Services.TabletInformationSender;
using WB.Core.GenericSubdomains.Logging;
using WB.Core.GenericSubdomains.Utils.Services;
using WB.Core.GenericSubdomains.Utils.Services.Rest;
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

        private readonly INetworkService networkService;
        private readonly IFileSystemAccessor fileSystemAccessor;
        private readonly ICapiInformationService capiInformationService;
        private readonly IRestService restService;
        private readonly IErrorReportingSettings errorReportingSettings;
        private readonly ILogger logger;
        
        public TabletInformationSender(ICapiInformationService capiInformationService, INetworkService networkService,
            IFileSystemAccessor fileSystemAccessor, IRestService restService, IErrorReportingSettings errorReportingSettings, ILogger logger)
        {
            this.capiInformationService = capiInformationService;
            this.networkService = networkService;
            this.fileSystemAccessor = fileSystemAccessor;

            this.restService = restService;
            this.errorReportingSettings = errorReportingSettings;
            this.logger = logger;
        }

        public event EventHandler<InformationPackageEventArgs> InformationPackageCreated;
        public event EventHandler ProcessCanceled;
        public event EventHandler ProcessFinished;

        public void Run()
        {
            this.tokenSource2 = new CancellationTokenSource();
            this.ct = this.tokenSource2.Token;
            this.task = Task.Factory.StartNew(this.RunInternal, this.ct);
        }

        public void Cancel()
        {
            if (this.tokenSource2.IsCancellationRequested)
                return;
            Task.Factory.StartNew(this.CancelInternal);
        }

        private void RunInternal()
        {
            if (!this.networkService.IsNetworkEnabled())
            {
                this.Cancel();
                return;
            }

            this.ExitIfCanceled();

            try
            {
                this.pathToInfoArchive = this.capiInformationService.CreateInformationPackage();


                if (string.IsNullOrEmpty(this.pathToInfoArchive) || !this.fileSystemAccessor.IsFileExists(this.pathToInfoArchive))
                {
                    this.OnProcessFinished();
                    return;
                }

                this.OnInformationPackageCreated(this.pathToInfoArchive, this.fileSystemAccessor.GetFileSize(this.pathToInfoArchive));

                this.ExitIfCanceled();

                var content = this.fileSystemAccessor.ReadAllBytes(this.pathToInfoArchive);

                var tabletInformationPackage = new TabletInformationPackage(this.fileSystemAccessor.GetFileName(this.pathToInfoArchive),
                    content,
                    this.errorReportingSettings.GetDeviceId(), this.errorReportingSettings.GetClientRegistrationId());

                try
                {
                    this.restService.PostAsync(url: "InterviewerSyncApi/PostInfoPackage", token: this.ct,
                        requestData: tabletInformationPackage);
                }
                catch
                {
                    throw new TabletInformationSendException("server didn't get information package");
                }
                finally
                {
                    this.fileSystemAccessor.DeleteFile(this.pathToInfoArchive);
                }
            }
            catch (Exception e)
            {
                this.logger.Error("Error occurred during the process. Process is being canceled.", e);
                this.Cancel();
                throw;
            }

            this.OnProcessFinished();
            this.DeleteInfoPackageIfExists();
        }

        private void OnProcessCanceled()
        {
            var handler = this.ProcessCanceled;
            if (handler != null)
                handler(this, EventArgs.Empty);
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

        private void CancelInternal()
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
            this.OnProcessCanceled();
            this.DeleteInfoPackageIfExists();
        }

        private void DeleteInfoPackageIfExists()
        {
            if (this.fileSystemAccessor.IsFileExists(this.pathToInfoArchive))
                this.fileSystemAccessor.DeleteFile(this.pathToInfoArchive);
        }
    }
}