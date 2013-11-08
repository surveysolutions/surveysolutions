using System;
using System.Collections.Generic;
using Android.Content;
using Android.Views;
using Android.Widget;
using CAPI.Android.Bindings;
using CAPI.Android.Core.Model.ViewModel.Login;
using Cirrious.CrossCore;
using Cirrious.CrossCore.Platform;
using Cirrious.MvvmCross.Binding.Bindings.Target.Construction;
using Cirrious.MvvmCross.Droid.Platform;
using Cirrious.MvvmCross.ViewModels;
using Cirrious.MvvmCross.Views;
using WB.UI.Capi.Shared;

namespace CAPI.Android
{
    public class Setup : CapiSharedSetup
    {
        public Setup(Context applicationContext) : base(applicationContext)
        {
        }

        protected override void InitializeViewLookup()
        {
            base.InitializeViewLookup();
            var container = Mvx.Resolve<IMvxViewsContainer>();
            container.Add(typeof(LoginViewModel), typeof(LoginActivity));
        }

        protected override IMvxApplication CreateApp()
        {
            return new App();
        }
    }
}