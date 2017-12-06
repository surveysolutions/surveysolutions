﻿using System;
using Android.Content.Res;
using Android.OS;
using Android.Support.V4.Widget;
using Android.Support.V7.App;
using Android.Views;
using MvvmCross.Platform;
using MvvmCross.Platform.Core;
using MvvmCross.Plugins.Messenger;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails;

namespace WB.UI.Shared.Enumerator.Activities
{
    public abstract class BaseInterviewActivity<TViewModel> : SingleInterviewActivity<TViewModel>
        where TViewModel : BaseInterviewViewModel
    {
        private ActionBarDrawerToggle drawerToggle;
        private DrawerLayout drawerLayout;
        private MvxSubscriptionToken sectionChangeSubscriptionToken;
        private MvxSubscriptionToken interviewCompleteActivityToken;

        protected override int ViewResourceId => Resource.Layout.interview;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            this.drawerLayout = this.FindViewById<DrawerLayout>(Resource.Id.drawer_layout);
            this.drawerToggle = new ActionBarDrawerToggle(this, this.drawerLayout, this.toolbar, 0, 0);
            this.drawerLayout.AddDrawerListener(this.drawerToggle);
            this.drawerLayout.DrawerOpened += (sender, args) =>
            {
                this.RemoveFocusFromEditText();
                this.HideKeyboard(drawerLayout.WindowToken);
            };
        }

        protected override void OnStart()
        {
            var messenger = Mvx.Resolve<IMvxMessenger>();

            this.sectionChangeSubscriptionToken = messenger.Subscribe<SectionChangeMessage>(this.OnSectionChange);
            this.interviewCompleteActivityToken = messenger.Subscribe<InterviewCompletedMessage>(this.OnInterviewCompleteActivity);
            base.OnStart();
        }

        public override void OnBackPressed()
        {
            this.ViewModel.NavigateToPreviousViewModel(() =>
            {
                this.ViewModel.NavigateBack();
                this.Finish();
            });
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
            => this.drawerToggle.OnOptionsItemSelected(item)
            || base.OnOptionsItemSelected(item);

        private void OnInterviewCompleteActivity(InterviewCompletedMessage obj)
        {
            try
            {
                this.Finish();
            }
            catch (ObjectDisposedException e)
            {
                Mvx.Resolve<ILoggerProvider>().GetForType(this.GetType()).Warn("Disposing already disposed activity", e);
            }
        }

        private void OnSectionChange(SectionChangeMessage msg) =>
            Mvx.Resolve<IMvxMainThreadDispatcher>().RequestMainThreadAction(() =>
            {
                try
                {
                    this.drawerLayout.CloseDrawers();
                }
                catch(ArgumentException)
                {
                    //ignore System.ArgumentExceptionHandle must be valid. Parameter name: instance
                }
            });

        protected override void OnPostCreate(Bundle savedInstanceState)
        {
            base.OnPostCreate(savedInstanceState);
            this.drawerToggle.SyncState();
        }

        public override void OnConfigurationChanged(Configuration newConfig)
        {
            base.OnConfigurationChanged(newConfig);
            this.drawerToggle.OnConfigurationChanged(newConfig);
        }

        public override void OnLowMemory()
        {
            base.OnLowMemory();
            GC.Collect(GC.MaxGeneration);
        }

        protected override void OnStop()
        {
            base.OnStop();

            var messenger = Mvx.Resolve<IMvxMessenger>();
            messenger.Unsubscribe<SectionChangeMessage>(sectionChangeSubscriptionToken);
            messenger.Unsubscribe<InterviewCompletedMessage>(interviewCompleteActivityToken);
            Mvx.Resolve<IAudioDialog>()?.StopRecordingAndSaveResult();
        }
    }
}