using System.Threading;
using System.Threading.Tasks;
using Android.Widget;
using Cirrious.MvvmCross.Droid.Fragging;

namespace WB.UI.Shared.Android.Activities
{
    public abstract class DoubleBackMvxFragmentActivity : MvxFragmentActivity
    {
        public override void OnBackPressed()
        {
            if (this.isPressAgaingMessageVisible)
            {
                this.HidePressAgainMessage();
                base.OnBackPressed();
            }
            else
            {
                this.ShowPressAgainMessage();
            }
        }

        private void HidePressAgainMessage()
        {
            this.pressAgainToast.Cancel();
        }

        private void ShowPressAgainMessage()
        {
            this.pressAgainToast = Toast.MakeText(this, "Press again to exit", ToastLength.Short);
            this.pressAgainToast.Show();

            this.isPressAgaingMessageVisible = true;
            Task.Factory.StartNew(() =>
            {
                Thread.Sleep(3000);
                this.isPressAgaingMessageVisible = false;
            });
        }

        private Toast pressAgainToast;
        private bool isPressAgaingMessageVisible = false;
    }
}