using System.Threading;
using System.Threading.Tasks;
using Android.App;

namespace WB.UI.Interviewer.Controls
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