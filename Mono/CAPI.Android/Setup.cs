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

namespace CAPI.Android
{
    public class Setup : MvxAndroidSetup
    {
        public Setup(Context applicationContext) : base(applicationContext)
        {
        }

        protected override void FillTargetFactories(IMvxTargetBindingFactoryRegistry registry)
        {
           

            registry.RegisterFactory(new MvxCustomBindingFactory<ViewGroup>("Background", (button) => new BackgroundBinding(button)));
            registry.RegisterFactory(new MvxCustomBindingFactory<TextView>("Html", (button) => new HtmlBinding(button)));
            registry.RegisterFactory(new MvxCustomBindingFactory<View>("Visible", (button) => new VisibilityBinding(button)));
            registry.RegisterFactory(new MvxCustomBindingFactory<TextView>("ValidationMessage", (button) => new ValidationMessageBinding(button)));

             base.FillTargetFactories(registry);

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