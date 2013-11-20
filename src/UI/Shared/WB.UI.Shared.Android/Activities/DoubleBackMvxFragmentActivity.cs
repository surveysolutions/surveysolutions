using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Cirrious.MvvmCross.Droid.Fragging;

namespace WB.UI.Shared.Android.Activities
{
    public abstract class DoubleBackMvxFragmentActivity : MvxFragmentActivity
    {
        public override void OnBackPressed()
        {
            if (isBackWasClickedRecently)
            {
                pressAgainToast.Cancel();
                base.OnBackPressed();
                return;
            }

            pressAgainToast = Toast.MakeText(this, "Press again to exit", ToastLength.Short);
            pressAgainToast.Show();

            isBackWasClickedRecently = true;
            Task.Factory.StartNew(() =>
            {
                Thread.Sleep(3000);
                isBackWasClickedRecently = false;
            });
        }

        private Toast pressAgainToast;
        private bool isBackWasClickedRecently = false;
    }
}