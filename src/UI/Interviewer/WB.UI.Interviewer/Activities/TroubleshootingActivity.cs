using System;
using System.Threading.Tasks;

using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Views;
using Android.Widget;
using Cirrious.CrossCore;
using Cirrious.CrossCore.Droid.Platform;
using Microsoft.Practices.ServiceLocation;
using Mono.CSharp;
using WB.Core.BoundedContexts.Interviewer.Properties;
using WB.Core.BoundedContexts.Interviewer.Services;
using WB.Core.BoundedContexts.Interviewer.Views;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.SharedKernels.Enumerator.Properties;
using WB.UI.Interviewer.Implementations.Services;
using WB.UI.Shared.Enumerator.Activities;
using Toolbar = Android.Support.V7.Widget.Toolbar;

namespace WB.UI.Interviewer.Activities
{
    [Activity(ConfigurationChanges = ConfigChanges.Orientation | ConfigChanges.KeyboardHidden | ConfigChanges.ScreenSize, Theme = "@style/GrayAppTheme")]
    public class TroubleshootingActivity : BaseActivity<TroubleshootingViewModel>
    {
        const string ApplicationFileName = "interviewer.apk";
        const string SyncGetlatestVersion = "/api/InterviewerSync/GetLatestVersion";

        private FilePickerFragment filePickerFragment;
        protected ProgressDialog progressDialog;
        private ProgressDialog progress;
        private PendingImplementation.Operation? currentOperation;
        private int clickCount = 0;
        const int NUMBER_CLICK = 10;
        
        protected TextView tvSyncResult => this.FindViewById<TextView>(Resource.Id.tvSyncResult);
        protected Button btnBackup => this.FindViewById<Button>(Resource.Id.btnBackup);
        protected Button btnSendTabletInfo => this.FindViewById<Button>(Resource.Id.btnSendTabletInfo);
        protected Button btnRestore => this.FindViewById<Button>(Resource.Id.btnRestore);
        protected LinearLayout llContainer => this.FindViewById<LinearLayout>(Resource.Id.llContainer);
        protected Button btnVersion => this.FindViewById<Button>(Resource.Id.btnVersion);

        private INetworkService networkService => ServiceLocator.Current.GetInstance<INetworkService>();
        private IInterviewerSettings interviewerSettings => ServiceLocator.Current.GetInstance<IInterviewerSettings>();
        private ISynchronizationService synchronizationService => ServiceLocator.Current.GetInstance<ISynchronizationService>();
        private ILogger logger => ServiceLocator.Current.GetInstance<ILogger>();

        protected override int ViewResourceId => Resource.Layout.sync_dialog;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            var toolbar = this.FindViewById<Toolbar>(Resource.Id.toolbar);
            toolbar.Title = "";
            this.SetSupportActionBar(toolbar);
            
            this.btnBackup.Click += delegate
            {
                filePickerFragment = new FilePickerFragment(null, null, FilePickerMode.Directory);
                filePickerFragment.FileSelected += async (sender, path) =>
                {
                    filePickerFragment.Dismiss();
                    await Task.Run(() => this.ViewModel.BackupCommand.Execute(path));
                };
                filePickerFragment.Cancel += sender => filePickerFragment.Dismiss();
                filePickerFragment.Show(FragmentManager, "FilePicker");
            };

            this.btnRestore.Click += delegate
            {
                filePickerFragment = new FilePickerFragment();
                filePickerFragment.FileSelected += (sender, path) =>
                {
                    filePickerFragment.Dismiss();

                    this.ViewModel.RestoreCommand.Execute(path);
                };
                filePickerFragment.Cancel += sender => filePickerFragment.Dismiss();
                filePickerFragment.Show(FragmentManager, "FilePicker");
            };

            this.btnSendTabletInfo.Click += this.btnSendTabletInfo_Click;
            this.tvSyncResult.Click += this.tvSyncResult_Click;
            this.llContainer.Click += this.llContainer_Click;
            this.btnVersion.Click += this.btnVersion_Click;
            this.btnVersion.Text = string.Format("Version: {0}. Check for a new version.", this.interviewerSettings.GetApplicationVersionName());

            this.ViewModel.PropertyChanged += ViewModel_PropertyChanged;
        }

        private void ViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "IsInProgress")
            {
                if (this.ViewModel.IsInProgress)
                    this.ShowProgressDialog(this.ViewModel.CanCancelProgress);
                else
                    this.HideProgressDialog();
            }
        }
        
        private void HideProgressDialog()
        {
            if (this.progressDialog == null) return;

            this.progressDialog.Hide();
            this.progressDialog.Dispose();
            this.progressDialog = null;
        }

        private void ShowProgressDialog(bool canCancelProcess)
        {
            this.progressDialog = new ProgressDialog(this);

            this.progressDialog.SetTitle(InterviewerUIResources.Troubleshooting_Old_InformationPackage);
            this.progressDialog.SetProgressStyle(ProgressDialogStyle.Spinner);
            this.progressDialog.SetMessage(InterviewerUIResources.Troubleshooting_Old_CreatingInformationPackage);

            this.progressDialog.SetCancelable(canCancelProcess);
            if (canCancelProcess)
                this.progressDialog.SetButton(UIResources.Cancel, this.TabletInformationSenderCanceled);

            this.progressDialog.Show();
        }

        private async void btnVersion_Click(object sender, EventArgs evnt)
        {
            if (!this.networkService.IsNetworkEnabled())
            {
                Toast.MakeText(this, "Network is unavailable", ToastLength.Long).Show();
                return;
            }
            this.progress = ProgressDialog.Show(this, "Checking", "Please Wait...", true, true);
            await this.CheckVersion();
        }

        private async Task CheckVersion()
        {
            bool? newVersionExists = null;
            try
            {
                var updater = new UpdateProcessor(logger: this.logger, synchronizationService: synchronizationService, interviewerSettings: this.interviewerSettings);
                newVersionExists = await updater.CheckNewVersion();
            }
            catch (Exception exc)
            {
                this.logger.Error("Error on new version check.", exc);
            }

            this.RunOnUiThread(() =>
            {
                if (this.progress != null)
                    this.progress.Dismiss();

                AlertDialog.Builder alert = new AlertDialog.Builder(this);
                alert.SetTitle("Checking for a new version");

                if (!newVersionExists.HasValue)
                {
                    alert.SetMessage("Error occurred on version check. Please, check settings or try again later.");
                }
                else if (newVersionExists.Value)
                {
                    alert.SetPositiveButton("Yes", this.btnUpdateConfirmed_Click);
                    alert.SetNegativeButton("No", this.btnUpdateDeclined_Click);
                    alert.SetMessage("New version exists. Would you like to download and update application?");
                }
                else
                {
                    alert.SetMessage("You have the latest version of application.");
                    alert.SetNegativeButton("Close", this.btnUpdateDeclined_Click);
                }

                alert.Show();
            });
        }

        private void btnUpdateDeclined_Click(object sender, DialogClickEventArgs e)
        {
        }

        private void btnUpdateConfirmed_Click(object sender, DialogClickEventArgs e)
        {
            var updater = new UpdateProcessor(logger: this.logger, synchronizationService: this.synchronizationService, interviewerSettings: this.interviewerSettings);
            this.progress = ProgressDialog.Show(this, "Downloading", "Please Wait...", true, true);

            Task.Factory.StartNew(() =>
            {
                try
                {
                    var uri = new Uri(new Uri(this.interviewerSettings.Endpoint), SyncGetlatestVersion);
                    updater.GetLatestVersion(uri, ApplicationFileName);
                    updater.StartUpdate(ApplicationFileName);
                }
                catch (Exception ex)
                {
                    this.logger.Error("Error on application update", ex);
                }
                finally
                {
                    this.RunOnUiThread(() =>
                    {
                        // hide the progress bar
                        if (this.progress != null)
                            this.progress.Dismiss();
                    });
                }
            });
        }

        void llContainer_Click(object sender, EventArgs e)
        {
            this.clickCount = 0;
        }

        private void tvSyncResult_Click(object sender, EventArgs e)
        {
            this.clickCount++;
            if (this.clickCount >= NUMBER_CLICK)
            {
                this.btnRestore.Visibility = ViewStates.Visible;
            }
        }

        private async void btnSendTabletInfo_Click(object sender, EventArgs e)
        {
            await Task.Run(() =>
            {
                this.ViewModel.SendTabletInformationCommand.Execute();
            });
        }

        private void TabletInformationSenderCanceled(object sender, DialogClickEventArgs e)
        {
            this.ViewModel.CancelSendingTabletInformationCommand.Execute();
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            this.ViewModel.PropertyChanged -= this.ViewModel_PropertyChanged;
            this.llContainer.Click -= this.llContainer_Click;
            this.btnVersion.Click -= this.btnVersion_Click;

            GC.Collect();
        }

        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            this.MenuInflater.Inflate(Resource.Menu.troubleshooting, menu);

            menu.LocalizeMenuItem(Resource.Id.menu_login, InterviewerUIResources.MenuItem_Title_Login);
            menu.LocalizeMenuItem(Resource.Id.menu_dashboard, InterviewerUIResources.MenuItem_Title_Dashboard);
            menu.LocalizeMenuItem(Resource.Id.menu_signout, InterviewerUIResources.MenuItem_Title_SignOut);
            menu.LocalizeMenuItem(Resource.Id.menu_settings, InterviewerUIResources.MenuItem_Title_Settings);

            var loginItem = menu.FindItem(Resource.Id.menu_login);
            var dashboardItem = menu.FindItem(Resource.Id.menu_dashboard);
            var singoutItem = menu.FindItem(Resource.Id.menu_signout);


            if (ViewModel.IsAuthenticated)
            {
                loginItem.SetVisible(false);
            }
            else
            {
                dashboardItem.SetVisible(false);
                singoutItem.SetVisible(false);
            }

            return base.OnCreateOptionsMenu(menu);
        }
        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            switch (item.ItemId)
            {
                case Resource.Id.menu_login:
                    this.ViewModel.NavigateToLoginCommand.Execute();
                    break;
                case Resource.Id.menu_dashboard:
                    this.ViewModel.NavigateToDashboardCommand.Execute();
                    break;
                case Resource.Id.menu_settings:
                    Intent intent = new Intent(this, typeof(PrefsActivity));
                    this.StartActivity(intent);
                    break;
                case Resource.Id.menu_signout:
                    this.ViewModel.SignOutCommand.Execute();
                    break;
            }
            return base.OnOptionsItemSelected(item);
        }
    }
}