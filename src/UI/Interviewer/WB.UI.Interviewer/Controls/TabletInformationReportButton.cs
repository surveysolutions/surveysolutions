using System;
using System.Threading;
using Android.App;
using Android.Content;
using Android.Util;
using Android.Widget;
using Ninject;
using WB.Core.BoundedContexts.Interviewer.ErrorReporting.Services.TabletInformationSender;
using WB.Core.GenericSubdomains.Portable;

namespace WB.UI.Interviewer.Controls
{
    public class TabletInformationReportButton : Button
    {
        private ITabletInformationSender tabletInformationSender;
        private Activity activity;

        protected ProgressDialog ProgressDialog;

        public TabletInformationReportButton(Context context)
            : base(context)
        {
            this.Init(context);
        }

        public TabletInformationReportButton(Context context, IAttributeSet attrs)
            : base(context, attrs)
        {
            this.Init(context);
        }

        public TabletInformationReportButton(Context context, IAttributeSet attrs, int defStyle)
            : base(context, attrs, defStyle)
        {
            this.Init(context);
        }

        private void Init(Context context)
        {
            this.activity = context as Activity;

            this.tabletInformationSender = InterviewerApplication.Kernel.Get<ITabletInformationSender>();

            this.Click += this.BtnSendTabletInfoClick;
        }

        public EventHandler ProcessFinished;
        public EventHandler<InformationPackageCancellationEventArgs> ProcessCanceled;
        public EventHandler InformationPackageCreated;
        public EventHandler SenderCanceled;

        private async void BtnSendTabletInfoClick(object sender, EventArgs e)
        {
            this.ThrowExceptionIfDialogIsOpened();

            this.tabletInformationSender.InformationPackageCreated += this.TabletInformationSenderInformationPackageCreated;
            this.tabletInformationSender.ProcessCanceled += this.TabletInformationSenderProcessCanceled;
            this.tabletInformationSender.ProcessFinished += this.TabletInformationSenderProcessFinished;

            this.DestroyDialog();
            this.ProgressDialog = new ProgressDialog(this.activity);

            this.ProgressDialog.SetTitle(this.Resources.GetText(Resource.String.InformationPackage));
            this.ProgressDialog.SetProgressStyle(ProgressDialogStyle.Spinner);
            this.ProgressDialog.SetMessage(this.Resources.GetText(Resource.String.CreatingInformationPackage));
            this.ProgressDialog.SetCancelable(false);

            this.ProgressDialog.SetButton(this.Resources.GetText(Resource.String.Cancel), this.TabletInformationSenderCanceled);

            this.ProgressDialog.Show();

            await this.tabletInformationSender.Run();
        }

        private void ThrowExceptionIfDialogIsOpened()
        {
            if (this.ProgressDialog != null)
                throw new InvalidOperationException();
        }

        private void DestroyDialog()
        {
            if (this.ProgressDialog == null)
                return;
            this.ProgressDialog.Dismiss();
            this.ProgressDialog.Dispose();
            this.ProgressDialog = null;
        }

        void TabletInformationSenderProcessFinished(object sender, EventArgs e)
        {
            this.activity.RunOnUiThread(() =>
            {
                this.DestroyDialog();
                if (this.ProcessFinished != null)
                {
                    this.ProcessFinished(this, e);
                }
            });
            this.DestroyReportSending();
        }

        void TabletInformationSenderProcessCanceled(object sender, InformationPackageCancellationEventArgs e)
        {
            this.activity.RunOnUiThread(() =>
            {
                this.DestroyDialog();

                if (this.ProcessCanceled != null)
                {
                    this.ProcessCanceled(this, e);
                }

                this.Enabled = true;
            });
            this.DestroyReportSending();
        }

        void TabletInformationSenderInformationPackageCreated(object sender, InformationPackageEventArgs e)
        {
            var remoteCommandDoneEvent = new AutoResetEvent(false);

            this.activity.RunOnUiThread(() =>
            {
                var builder = new AlertDialog.Builder(this.activity);

                builder.SetMessage(
                    string.Format(this.Resources.GetText(Resource.String.InformationPackageSizeWarningFormat),
                        FileSizeUtils.SizeSuffix(e.FileSize)));

                builder.SetPositiveButton(this.Resources.GetText(Resource.String.Yes), (s, positiveEvent) =>
                {
                    this.ProgressDialog.SetMessage(this.Resources.GetText(Resource.String.SendingInformationPackage)); remoteCommandDoneEvent.Set(); });
                builder.SetNegativeButton(this.Resources.GetText(Resource.String.No), (s, negativeEvent) =>
                {
                    this.tabletInformationSender.Cancel();
                    remoteCommandDoneEvent.Set();
                });
                builder.Show();

                if (this.InformationPackageCreated != null)
                {
                    this.InformationPackageCreated(this, e);
                }
            });

            remoteCommandDoneEvent.WaitOne();
        }

        private void TabletInformationSenderCanceled(object sender, DialogClickEventArgs e)
        {
            this.tabletInformationSender.Cancel();
            if (this.SenderCanceled != null)
            {
                this.SenderCanceled(this, e);
            }
            this.Enabled = false;
        }

        private void DestroyReportSending()
        {
            this.tabletInformationSender.InformationPackageCreated -= this.TabletInformationSenderInformationPackageCreated;
            this.tabletInformationSender.ProcessCanceled -= this.TabletInformationSenderProcessCanceled;
            this.tabletInformationSender.ProcessFinished -= this.TabletInformationSenderProcessFinished;
        }
    }
}