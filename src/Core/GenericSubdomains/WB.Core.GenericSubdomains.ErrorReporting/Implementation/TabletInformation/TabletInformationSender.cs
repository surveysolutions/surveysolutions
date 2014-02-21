using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Practices.ServiceLocation;
using WB.Core.GenericSubdomain.Rest;
using WB.Core.GenericSubdomains.ErrorReporting.Services.CapiInformationService;
using WB.Core.GenericSubdomains.ErrorReporting.Services.TabletInformationSender;
using WB.Core.GenericSubdomains.Logging;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.SharedKernel.Structures.TabletInformation;
using WB.Core.SharedKernel.Utils.Serialization;

namespace WB.Core.GenericSubdomains.ErrorReporting.Implementation.TabletInformation
{
    internal class TabletInformationSender : ITabletInformationSender
    {
        private ILogger Logger
        {
            get { return ServiceLocator.Current.GetInstance<ILogger>(); }
        }
        
        private CancellationToken ct;
        private CancellationTokenSource tokenSource2;
        private Task task;
        private string pathToInfoArchive = null;

        private readonly INetworkService networkService;
        private readonly IFileSystemAccessor fileSystemAccessor;
        private readonly ICapiInformationService capiInformationService;
        private readonly IRestServiceWrapper webExecutor;

        private readonly IJsonUtils jsonUtils;
        private readonly string registrationKeyName;
        private readonly string androidId;

        private const string PostInfoPackagePath = "TabletReport/PostInfoPackage";

        public TabletInformationSender(ICapiInformationService capiInformationService, INetworkService networkService,
            IFileSystemAccessor fileSystemAccessor, IJsonUtils jsonUtils, string syncAddressPoint, string registrationKeyName, string androidId, IRestServiceWrapperFactory restServiceWrapperFactory)
        {
            this.capiInformationService = capiInformationService;
            this.networkService = networkService;
            this.fileSystemAccessor = fileSystemAccessor;
            this.jsonUtils = jsonUtils;

            this.registrationKeyName = registrationKeyName;
            this.androidId = androidId;

            this.webExecutor = restServiceWrapperFactory.CreateRestServiceWrapper(syncAddressPoint);
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
            }

            this.ExitIfCanceled();

            this.CancelIfException(() => { this.pathToInfoArchive = this.capiInformationService.CreateInformationPackage(); });

            if (string.IsNullOrEmpty(this.pathToInfoArchive) || !this.fileSystemAccessor.IsFileExists(this.pathToInfoArchive))
            {
                this.OnProcessFinished();
                return;
            }

            this.OnInformationPackageCreated(this.pathToInfoArchive, this.fileSystemAccessor.GetFileSize(this.pathToInfoArchive));

            this.ExitIfCanceled();

            this.CancelIfException(() =>
            {
                var content = this.fileSystemAccessor.ReadAllBytes(this.pathToInfoArchive);

                var tabletInformationPackage = new TabletInformationPackage(this.fileSystemAccessor.GetFileName(this.pathToInfoArchive), content,
                    this.androidId, this.registrationKeyName);

                var result = this.webExecutor.ExecuteRestRequestAsync<bool>(PostInfoPackagePath, this.ct,
                    this.jsonUtils.GetItemAsContent(tabletInformationPackage), null, null, null);

                this.fileSystemAccessor.DeleteFile(this.pathToInfoArchive);

                if (!result)
                    throw new TabletInformationSendException("server didn't get information package");
            });

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

        private void CancelIfException(Action action)
        {
            try
            {
                action();
            }
            catch (Exception exc)
            {
                this.Logger.Error("Error occurred during the process. Process is being canceled.", exc);
                this.Cancel();
                throw;
            }
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
                    this.Logger.Error("Error occurred during the process. Process is being canceled.", exception);
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