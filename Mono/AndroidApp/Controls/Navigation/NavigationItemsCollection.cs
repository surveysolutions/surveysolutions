using System;
using System.Collections.Generic;

using Android.App;
using Android.Content;
using Android.Views;
using Android.Widget;
using AndroidApp.Core.Model.Authorization;

namespace AndroidApp.Controls.Navigation
{
    using Android.OS;

    using AndroidApp.Services;

    using AndroidMain.Synchronization;

    using Main.Synchronization.SyncManager;
    using Main.Synchronization.SyncStreamCollector;

    //using Ionic.Zip;

    public class NavigationItemsCollection : List<NavigationItem>
    {
        private SyncServiceBinder syncServiceBinder;

        private readonly Context context;
        private readonly IAuthentication membership;

        public NavigationItemsCollection(Context context)
        {
            this.context = context;
            this.membership = CapiApplication.Membership;

            this.Add(new NavigationItem(Dashboard, "Dashboard"));
            this.Add(new NavigationItem(Synchronization, "Synchronization"));

            if (this.membership.IsLoggedIn)
            {
                this.Add(new NavigationItem(LogOff, "LogOff"));
            }
        }

        protected bool LogOff(object sender, EventArgs e)
        {
            this.membership.LogOff();
            this.context.StartActivity(typeof(LoginActivity));
            return true;
        }

        protected bool Dashboard(object sender, EventArgs e)
        {
            if (this.context is DashboardActivity || this.context is LoginActivity)
            {
                return true;
            }

            this.context.StartActivity(
                this.membership.IsLoggedIn ? 
                typeof(DashboardActivity) : 
                typeof(LoginActivity));
            
            return true;
        }

        /*private void CancelClicked(object sender, DialogClickEventArgs dialogClickEventArgs)
        {
            if (membership.IsLoggedIn)
                this.context.StartActivity(typeof(DashboardActivity));
            else
                this.context.StartActivity(typeof(LoginActivity));
        }

        private void OkClicked(object sender, DialogClickEventArgs dialogClickEventArgs)
        {
            var dialog = sender as AlertDialog;

            if (null != dialog)
            {

                Guid processKey = Guid.NewGuid();
                string remoteSyncNode = "http://217.12.197.135/DEV-Supervisor/";
                string syncMessage = "Remote sync.";
                try
                {
                    var streamProvider = new RemoteServiceEventStreamProvider1(CapiApplication.Kernel, processKey, remoteSyncNode);
                    var collector = new LocalStorageStreamCollector(CapiApplication.Kernel, processKey);

                    var manager = new SyncManager(streamProvider, collector, processKey, syncMessage, null);
                    manager.StartPush();
                }
                catch (Exception ee)
                {
                    //return false;
                }


                /*var connectionEdit = dialog.FindViewById(Resource.Id.connectionstring_edit) as EditText;


                if (null != connectionEdit)
                    Console.WriteLine("Connection String: {0}", connectionEdit.Text);
#1#
            }
        }*/

        protected bool Synchronization(object sender, EventArgs e)
        {

            this.context.StartActivity(typeof(SynchronizationActivity));

            return true;
        }

        protected bool Sync(object sender, EventArgs e)
        {

            /*
                        var builder = new AlertDialog.Builder(context);
                        builder.SetMessage("Synchronization");

                        builder.SetNegativeButton("Cancel", delegate(object o, DialogClickEventArgs args) { CancelClicked(o, args); });

                        builder.SetPositiveButton("OK", delegate(object o, DialogClickEventArgs args) { OkClicked(o, args); });
            
            
                        builder.Create();
                        builder.Show();
                        return false;
            */


            /*LayoutInflater layoutInflater = (LayoutInflater)context.GetSystemService(Context.LayoutInflaterService);
            var sync_dialog_view = layoutInflater.Inflate(Resource.Layout.sync_dialog, null);*/
            
            this.context.StartActivity(typeof(SyncActivity));

            return true;

            /*string messageText = string.Empty;

            try
            {
                var address = "http://217.12.197.135/DEV-Supervisor/";
                var syncId = Guid.NewGuid();
                var kernel = CapiApplication.Kernel;
                var provider = new RemoteServiceEventStreamProvider1(kernel, syncId, address);
                var collector = new LocalStorageStreamCollector(kernel, syncId);
                var synkMngr = new SyncManager(provider, collector, syncId, "Remote server sync.", null);
                synkMngr.StartPush();

                messageText += "Sync in is OK.";
            }
            catch (Exception exc)
            {
                messageText += "Error on sync. " + exc.Message;
            }

            try
            {
                var syncId = Guid.NewGuid();
                var kernel = CapiApplication.Kernel;

                var provider = new AllIntEventsStreamProvider();
                var collector = new CompressedStreamStreamCollector(syncId);
                
                var synkMngr = new SyncManager(provider, collector, syncId, "Local backup. ", null);
                synkMngr.StartPush();

                var documents = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

                var filename = Path.Combine(documents, "Write.zip");

                File.WriteAllBytes(filename, collector.GetExportedStream().ToArray());

                messageText += filename;

                messageText += "Sync out is OK.";
            }
            catch (Exception exc)
            {
                messageText += "Error on sync out. " + exc.Message;
            }

            builder.SetMessage(messageText);

/*

            var zip = new ZipFile();
            zip.AddEntry("backup.txt", "It's a compression test.");
            var outputStream = new MemoryStream();
            zip.Save(outputStream);

            builder.SetMessage("Sync. Result length: " + outputStream.Length);
#1#


            builder.Show();*/

            // return false;
        }
    }
}