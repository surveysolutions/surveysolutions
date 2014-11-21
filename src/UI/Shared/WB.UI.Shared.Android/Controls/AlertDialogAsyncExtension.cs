using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.OS;
using Android.OS.Storage;
using Android.Runtime;
using Android.Views;
using Android.Widget;

namespace WB.UI.Shared.Android.Controls
{
    public static class AlertDialogAsyncExtension
    {
        public static async Task Confirmation(this AlertDialog dialog)
        {
            await Task.Factory.StartNew(() =>
            {
                while (dialog.IsShowing)
                {
                    Thread.Sleep(200);
                }
                dialog.Dispose();
                dialog = null;
            });
        }
    }
}