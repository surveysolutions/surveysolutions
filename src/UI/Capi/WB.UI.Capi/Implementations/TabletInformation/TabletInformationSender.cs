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
using WB.Core.SharedKernel.Structures.TabletInformation;
using WB.UI.Capi.Settings;
using WB.UI.Capi.Utils;
using WB.UI.Shared.Android.RestUtils;
using Path = System.IO.Path;

namespace WB.UI.Capi.Implementations.TabletInformation
{
    public class TabletInformationSender
    {
        private ILogger Logger
        {
            get { return ServiceLocator.Current.GetInstance<ILogger>(); }
        }
        
        private CancellationToken ct;
        private CancellationTokenSource tokenSource2;
        private Task task;
        
        private readonly Context context;
        private readonly ICapiInformationService capiInformationService;
        private readonly IRestUrils webExecutor;
        private const string postInfoPackagePath = "TabletReport/PostInfoPackage";

        public TabletInformationSender(Context context, ICapiInformationService capiInformationService)
        {
            this.context = context;
            this.capiInformationService = capiInformationService;
            this.webExecutor = new AndroidRestUrils(SettingsManager.GetSyncAddressPoint());;
        }

        public event EventHandler InformationPackageCreated;
        public event EventHandler ProcessCanceled;
        public event EventHandler ProcessFinished;

        public void Run()
        {
            this.tokenSource2 = new CancellationTokenSource();
            this.ct = this.tokenSource2.Token;
            this.task = Task.Factory.StartNew(this.RunInternal, this.ct);
        }

        private void RunInternal()
        {
            if (!NetworkHelper.IsNetworkEnabled(context))
            {
                return;
            }

            ExitIfCanceled();

            string pathToInfoArchive = null;
            this.CancelIfException(() => { pathToInfoArchive = capiInformationService.CreateInformationPackage(); });

            if (string.IsNullOrEmpty(pathToInfoArchive) || !System.IO.File.Exists(pathToInfoArchive))
            {
                OnProcessFinished();
                return;
            }

            OnInformationPackageCreated();

            ExitIfCanceled();

            this.CancelIfException(() =>
            {
                var content = System.IO.File.ReadAllBytes(pathToInfoArchive);

                var tabletInformationPackage = new TabletInformationPackage(Path.GetFileName(pathToInfoArchive), content,
                    SettingsManager.AndroidId, SettingsManager.GetSetting(SettingsNames.RegistrationKeyName));

                var result = this.webExecutor.ExcecuteRestRequestAsync<bool>(postInfoPackagePath, ct,
                    JsonUtils.GetJsonData(tabletInformationPackage), null, null);

                if(!result)
                    throw new NotImplementedException("");
            });

            OnProcessFinished();
        }

        protected void OnProcessCanceled()
        {
            var handler = this.ProcessCanceled;
            if (handler != null)
                handler(this, EventArgs.Empty);
        }

        protected void OnInformationPackageCreated()
        {
            if (this.tokenSource2.IsCancellationRequested)
                return;
            var handler = this.InformationPackageCreated;
            if (handler != null)
                handler(this, EventArgs.Empty);
        }

        protected void OnProcessFinished()
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

        public void Cancel()
        {
            if (this.tokenSource2.IsCancellationRequested)
                return;
            Task.Factory.StartNew(this.CancelInternal);
        }

        private void CancelInternal()
        {
       //     this.OnProcessCanceling();

            this.tokenSource2.Cancel();

         //   List<Exception> exceptions = new List<Exception>();
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
              //  exceptions = e.InnerExceptions.ToList();
            }
       //     exceptions.Add(new Exception("Synchronization wasn't completed"));
            this.OnProcessCanceled();
        }
    }
}