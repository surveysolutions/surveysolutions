using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Android.Content;
using Android.Graphics;
using Android.Media;
using Android.Widget;
using CAPI.Android.Core.Model;
using CAPI.Android.Settings;
using Java.IO;
using Microsoft.Practices.ServiceLocation;
using RestSharp;
using WB.Core.BoundedContexts.Capi.ModelUtils;
using WB.Core.GenericSubdomains.Logging;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.SharedKernel.Structures.TabletInformation;
using WB.Core.SharedKernel.Utils.Serialization;
using WB.UI.Capi.Services;
using WB.UI.Capi.Settings;
using WB.UI.Capi.Utils;
using WB.UI.Shared.Android.Network;
using WB.UI.Shared.Android.RestUtils;

namespace WB.UI.Capi.Implementations.TabletInformation
{
    public class TabletInformationSender : ITabletInformationSender
    {
        private ILogger Logger
        {
            get { return ServiceLocator.Current.GetInstance<ILogger>(); }
        }
        
        private CancellationToken ct;
        private CancellationTokenSource tokenSource2;
        private Task task;
        private string pathToInfoArchive = null;

        private readonly ICapiNetworkService capiNetworkService;
        private readonly IFileSystemAccessor fileSystemAccessor;
        private readonly ICapiInformationService capiInformationService;
        private readonly IRestUrils webExecutor;
        private readonly IJsonUtils jsonUtils;

        private const string PostInfoPackagePath = "TabletReport/PostInfoPackage";

        public TabletInformationSender(ICapiInformationService capiInformationService, ICapiNetworkService capiNetworkService,
            IFileSystemAccessor fileSystemAccessor, IJsonUtils jsonUtils)
        {
            this.capiInformationService = capiInformationService;
            this.capiNetworkService = capiNetworkService;
            this.fileSystemAccessor = fileSystemAccessor;
            this.jsonUtils = jsonUtils;

            this.webExecutor = new AndroidRestUrils(SettingsManager.GetSyncAddressPoint());
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
            if (!capiNetworkService.IsNetworkEnabled())
            {
                this.Cancel();
            }

            ExitIfCanceled();

            this.CancelIfException(() => { pathToInfoArchive = capiInformationService.CreateInformationPackage(); });

            if (string.IsNullOrEmpty(pathToInfoArchive) || !fileSystemAccessor.IsFileExists(pathToInfoArchive))
            {
                OnProcessFinished();
                return;
            }

            OnInformationPackageCreated(pathToInfoArchive, fileSystemAccessor.GetFileSize(pathToInfoArchive));

            ExitIfCanceled();

            this.CancelIfException(() =>
            {
                var content = fileSystemAccessor.ReadAllBytes(pathToInfoArchive);

                var tabletInformationPackage = new TabletInformationPackage(fileSystemAccessor.GetFileName(pathToInfoArchive), content,
                    SettingsManager.AndroidId, SettingsManager.GetSetting(SettingsNames.RegistrationKeyName));

                var result = this.webExecutor.ExcecuteRestRequestAsync<bool>(PostInfoPackagePath, ct,
                    jsonUtils.GetItemAsContent(tabletInformationPackage), null, null);

                fileSystemAccessor.DeleteFile(pathToInfoArchive);

                if (!result)
                    throw new TabletInformationSendException("server didn't get information package");
            });

            OnProcessFinished();
            DeleteInfoPackageIfExists();
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
            if (fileSystemAccessor.IsFileExists(this.pathToInfoArchive))
                fileSystemAccessor.DeleteFile(this.pathToInfoArchive);
        }
    }
}