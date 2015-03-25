using System.Collections.Generic;
using System.Reflection;
using Android.Content;
using Android.Views;
using Android.Widget;
using Cirrious.CrossCore.IoC;
using Cirrious.MvvmCross.Binding.Bindings.Target.Construction;
using Cirrious.MvvmCross.Droid.Platform;
using Cirrious.MvvmCross.ViewModels;
using WB.Core.BoundedContexts.QuestionnaireTester.ViewModels;
using WB.UI.QuestionnaireTester.Controls;
using WB.UI.QuestionnaireTester.Mvvm.CustomBindings;
using WB.UI.QuestionnaireTester.Ninject;
using Xamarin;

namespace WB.UI.QuestionnaireTester
{
    public class Setup : MvxAndroidSetup
    {
        public Setup(Context applicationContext) : base(applicationContext)
        {
            this.InitializeLogger(applicationContext);
        }

        protected override IMvxIoCProvider CreateIocProvider()
        {
            return NinjectIoCAdapterSetup.CreateIocProvider();
        }

        protected override IMvxApplication CreateApp()
        {
            return new App();
        }

        protected override void FillTargetFactories(IMvxTargetBindingFactoryRegistry registry)
        {
            registry.RegisterFactory(new MvxCustomBindingFactory<TextView>("Html", (button) => new HtmlBinding(button)));
            registry.RegisterFactory(new MvxCustomBindingFactory<View>("Visible", (button) => new VisibleBinding(button)));
            registry.RegisterFactory(new MvxCustomBindingFactory<MvxSwipeRefreshLayout>("RefreshCommand", (view) => new SwipeRefreshLayoutRefreshBinding(view)));
            registry.RegisterFactory(new MvxCustomBindingFactory<MvxSwipeRefreshLayout>("Refreshing", (view) => new SwipeRefreshLayoutRefreshingBinding(view)));
            registry.RegisterFactory(new MvxCustomBindingFactory<SearchView>("QueryTextChange", (view) => new SearchViewQueryTextChangeBinding(view)));
            registry.RegisterFactory(new MvxCustomBindingFactory<SearchView>("QueryTextSubmit", (view) => new SearchViewQueryTextSubmitBinding(view)));
            registry.RegisterFactory(new MvxCustomBindingFactory<SearchView>("QueryHint", (view) => new SearchViewQueryHintBinding(view)));
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