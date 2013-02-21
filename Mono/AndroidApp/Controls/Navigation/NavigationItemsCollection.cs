using System;
using System.Collections.Generic;

using Android.Content;
using AndroidApp.Core.Model.Authorization;

namespace AndroidApp.Controls.Navigation
{
    public class NavigationItemsCollection : List<NavigationItem>
    {
        private readonly Context context;
        private readonly IAuthentication membership;

        public int? SelectedItemIndex;

        public NavigationItemsCollection(Context context)
        {
            this.context = context;

            this.membership = CapiApplication.Membership;

            if (this.membership.IsLoggedIn)
            {
                this.Add(new NavigationItem(Dashboard, "Dashboard"));
                if (context is DashboardActivity)
                {
                    this.SelectedItemIndex = 0;
                }

                this.Add(new NavigationItem(Synchronization, "Synchronization"));
                if (context is SynchronizationActivity)
                {
                    this.SelectedItemIndex = 1;
                }

                this.Add(new NavigationItem(Settings, "Settings"));
                if (context is SettingsActivity)
                {
                    this.SelectedItemIndex = 2;
                }

                this.Add(new NavigationItem(LogOff, "LogOff"));

            }
            else
            {
                this.Add(new NavigationItem(Login, "LogIn"));
                if (context is LoginActivity)
                {
                    this.SelectedItemIndex = 0;
                }

                this.Add(new NavigationItem(Synchronization, "Synchronization"));
                if (context is SynchronizationActivity)
                {
                    this.SelectedItemIndex = 1;
                }

                this.Add(new NavigationItem(Settings, "Settings"));
                if (context is SettingsActivity)
                {
                    this.SelectedItemIndex = 2;
                }
            }
        }

        private bool Settings(object arg1, EventArgs arg2)
        {
            this.context.StartActivity(typeof(SettingsActivity));
            return true;
        }

        private bool Login(object arg1, EventArgs arg2)
        {
            this.context.StartActivity(typeof(LoginActivity));
            return true;
        }

        protected bool LogOff(object sender, EventArgs e)
        {
            this.membership.LogOff();
            this.context.StartActivity(typeof(LoginActivity));
            return true;
        }

        protected bool Dashboard(object sender, EventArgs e)
        {
            /*if (this.context is DashboardActivity || this.context is LoginActivity)
            {
                return true;
            }

            this.context.StartActivity(
                this.membership.IsLoggedIn ? 
                typeof(DashboardActivity) : 
                typeof(LoginActivity));
            */

            this.context.StartActivity(typeof(SynchronizationActivity));
            return true;
        }

        protected bool Synchronization(object sender, EventArgs e)
        {
            this.context.StartActivity(typeof(SynchronizationActivity));

            return true;
        }
    }
}