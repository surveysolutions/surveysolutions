using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using CAPI.Android.Extensions;
using CAPI.Android.Syncronization;
using CAPI.Android.Utils;

namespace CAPI.Android
{
    [Activity(Icon = "@drawable/capi",
        ConfigurationChanges = ConfigChanges.Orientation | ConfigChanges.KeyboardHidden | ConfigChanges.ScreenSize)]
    public class SynchronizationActivity : Activity
    {
        #region find for ui controls from xml

        protected TextView tvSyncResult
        {
            get { return this.FindViewById<TextView>(Resource.Id.tvSyncResult); }
        }

        protected ProgressDialog progressDialog;
        #endregion


        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            this.SetContentView(Resource.Layout.sync_dialog);

            var buttonSync = this.FindViewById<Button>(Resource.Id.btnSync);
            if (buttonSync != null)
            {
                buttonSync.Click += this.ButtonSyncClick;
                buttonSync.Enabled = NetworkHelper.IsNetworkEnabled(this);
            }
        }

        protected override void OnStart()
        {
            base.OnStart();
            this.CreateActionBar();
        }

        private void ButtonSyncClick(object sender, EventArgs e)
        {
            ThrowExeptionIfDialogIsOpened();

            PreperaUI();

            SynchronozationProcessor synchronizer;
            try
            {
                 synchronizer = new SynchronozationProcessor(this);
            }
            catch (Exception ex)
            {
                tvSyncResult.Text = ex.Message;
                return;
            }

            synchronizer.StatusChanged += (s, evt) => this.RunOnUiThread(() => synchronizer_StatusChanged(s, evt));
            synchronizer.ProcessFinished += (s, evt) => this.RunOnUiThread(() => synchronizer_ProcessFinished(s, evt));

            CreateDialog(ProgressDialogStyle.Spinner, "Initializing");

            synchronizer.Run();
        }

        private void PreperaUI()
        {
            tvSyncResult.Text = string.Empty;
        }

        private void ThrowExeptionIfDialogIsOpened()
        {
            if (progressDialog != null)
                throw new InvalidOperationException();
        }

        private void synchronizer_ProcessFinished(object sender, EventArgs e)
        {
            DestroyDialog();
        }

        private void synchronizer_StatusChanged(object sender, SynchronizationEvent e)
        {
            var messageWithPersents = e as SynchronizationEventWithPercent;
            var currentStyle = messageWithPersents == null
                                   ? ProgressDialogStyle.Spinner
                                   : ProgressDialogStyle.Horizontal;
            if (currentStyle != dialogStyle)
            {
                DestroyDialog();
                CreateDialog(currentStyle, e.OperationTitle);
            }
            else
            {
                progressDialog.SetMessage(e.OperationTitle);
            }

            if (messageWithPersents != null)
            {
                progressDialog.Progress = messageWithPersents.Percent;
            }

        }

        private ProgressDialogStyle dialogStyle;
        #region diialog manipulation

        private void CreateDialog(ProgressDialogStyle style, string title)
        {
            dialogStyle = style;
            progressDialog = new ProgressDialog(this);
            
            progressDialog.SetTitle("Synchronizing");
            progressDialog.SetProgressStyle(dialogStyle);
            progressDialog.SetMessage(title);
            progressDialog.SetCancelable(false);
            progressDialog.Show();
        }

        private void DestroyDialog()
        {
            if (progressDialog == null)
                return;
            progressDialog.Dismiss();

            progressDialog.Dispose();
            progressDialog = null;
        }

        #endregion
    }
}