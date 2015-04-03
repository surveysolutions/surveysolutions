using System;
using System.Collections.Generic;
using System.Reflection;
using Android.Content;
using Android.Widget;
using Cirrious.CrossCore;
using Cirrious.CrossCore.IoC;
using Cirrious.MvvmCross.Binding.Bindings.Target.Construction;
using Cirrious.MvvmCross.Droid.Platform;
using Cirrious.MvvmCross.ViewModels;
using Cirrious.MvvmCross.Views;
using WB.Core.BoundedContexts.QuestionnaireTester.ViewModels;
using WB.UI.QuestionnaireTester.CustomBindings;
using WB.UI.QuestionnaireTester.Ninject;
using WB.UI.QuestionnaireTester.Views;
using Xamarin;

namespace WB.UI.QuestionnaireTester
{
    public class Setup : MvxAndroidSetup
    {
        public Setup(Context applicationContext) : base(applicationContext)
        {
            this.InitializeLogger(applicationContext);
        }

        protected override IMvxApplication CreateApp()
        {
            return new App();
        }

        protected override IMvxIoCProvider CreateIocProvider()
        {
            return NinjectIoCAdapterSetup.CreateIocProvider();
        }

        protected override void InitializeViewLookup()
        {
            var viewModelViewLookup = new Dictionary<Type, Type>()
            {
                {typeof (SplashViewModel), typeof (SplashView)},
                {typeof (LoginViewModel), typeof (LoginView)}
            };

            var container = Mvx.Resolve<IMvxViewsContainer>();
            container.AddAll(viewModelViewLookup);
        }

        protected override void FillTargetFactories(IMvxTargetBindingFactoryRegistry registry)
        {
            registry.RegisterFactory(new MvxCustomBindingFactory<TextView>("Hint", (view) => new TextViewHintBinding(view)));

            base.FillTargetFactories(registry);
        }

        protected override Assembly[] GetViewModelAssemblies()
        {
            return new[] { typeof(BaseViewModel).Assembly };
        }

        private void InitializeLogger(Context applicationContext)
        {
            Insights.HasPendingCrashReport += (sender, isStartupCrash) =>
            {
                if (isStartupCrash)
                {
                    Insights.PurgePendingCrashReports().Wait();
                }
            };
            Insights.Initialize("24d22f99f3068798f24f20d297baaa0fbfe9f528", applicationContext);
        }
    }
}