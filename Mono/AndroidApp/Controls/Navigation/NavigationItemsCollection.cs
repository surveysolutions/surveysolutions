using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using AndroidApp.Authorization;

namespace AndroidApp.Controls.Navigation
{
    public class NavigationItemsCollection : List<NavigationItem>
    {
        private readonly Context context;
        private readonly IAuthentication membership;

        public NavigationItemsCollection(Context context)
        {
            this.context = context;
            this.membership = CapiApplication.Membership;
            Add(new NavigationItem(Dashboard, "Dashboard"));
            Add(new NavigationItem(Synchronization, "Synchronization"));
            if (membership.IsLoggedIn)
                Add(new NavigationItem(LogOff, "LogOff"));
        }

        protected bool LogOff(object sender, EventArgs e)
        {
            membership.LogOff();
            this.context.StartActivity(typeof (LoginActivity));
            return true;
        }
        protected bool Dashboard(object sender, EventArgs e)
        {
            if(this.context is DashboardActivity || this.context is LoginActivity)
                return true;
            if (membership.IsLoggedIn)
                this.context.StartActivity(typeof (DashboardActivity));
            else
                this.context.StartActivity(typeof(LoginActivity));
            return true;
        }
        protected bool Synchronization(object sender, EventArgs e)
        {
            var builder = new AlertDialog.Builder(context);
            builder.SetMessage("Sync");
            builder.Show();
            return false;
        }
    }
}