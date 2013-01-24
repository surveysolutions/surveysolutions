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
using AndroidApp.Core.Model.Authorization;

namespace AndroidApp.Controls.Navigation
{
    using System.IO;

    using Ionic.Zip;

    using Main.Synchronization.SyncManager;
    using Main.Synchronization.SyncSreamProvider;
    using Main.Synchronization.SyncStreamCollector;

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

            builder.SetMessage("Synchronization will be here.");


            /*try
            {
                var address = "http://217.12.197.135/DEV-Supervisor/importexport/export";
                var syncId = Guid.NewGuid();
                var kernel = CapiApplication.Kernel;
                var provider = new RemoteServiceEventStreamProvider(kernel, syncId, address);
                var collector = new LocalStorageStreamCollector(kernel, syncId);
                var synkMngr = new SyncManager(provider, collector, syncId, "remote sync", null);
                synkMngr.StartPush();

                builder.SetMessage("Sync is OK.");
            }
            catch (Exception exc)
            {
                builder.SetMessage("Error on sync. " + exc.Message);
            }*/

/*

            var zip = new ZipFile();
            zip.AddEntry("backup.txt", "It's a compression test.");
            var outputStream = new MemoryStream();
            zip.Save(outputStream);

            builder.SetMessage("Sync. Result length: " + outputStream.Length);
*/


            builder.Show();
            return false;
        }
    }
}