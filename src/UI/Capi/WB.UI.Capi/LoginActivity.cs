using System;
using System.Collections.Generic;

using Android.App;
using Android.Graphics;
using Android.OS;
using Android.Widget;
using Android.Content.PM;
using Cirrious.MvvmCross.Droid.Views;
using WB.UI.Capi.Extensions;
using WB.UI.Capi.Views;

namespace WB.UI.Capi
{
    [Activity(ConfigurationChanges = ConfigChanges.Orientation | ConfigChanges.KeyboardHidden | ConfigChanges.ScreenSize)]
    public class LoginActivity : MvxActivity
    {
        protected override void OnStart()
        {
            base.OnStart();
            this.CreateActionBar();
        }

        protected override void OnCreate(Bundle bundle)
        {
            this.DataContext = new LoginActivityViewModel();
            base.OnCreate(bundle);
            this.SetContentView(Resource.Layout.Login);
        }
    }
}