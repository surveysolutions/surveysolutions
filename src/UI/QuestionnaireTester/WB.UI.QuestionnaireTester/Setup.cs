using System.Reflection;
using Android.Content;
using Android.Widget;
using Cirrious.CrossCore;
using Cirrious.MvvmCross.Binding.Bindings.Target.Construction;
using Cirrious.MvvmCross.ViewModels;
using WB.Core.BoundedContexts.QuestionnaireTester.ViewModels;
using WB.UI.QuestionnaireTester.Controls;
using WB.UI.QuestionnaireTester.CustomBindings;
using WB.UI.Shared.Android;
using Xamarin;

namespace WB.UI.QuestionnaireTester
{
    public class Setup : CapiSharedSetup
    {
        public Setup(Context applicationContext) : base(applicationContext)
        {
            Insights.Initialize("24d22f99f3068798f24f20d297baaa0fbfe9f528", applicationContext);
        }

        protected override IMvxApplication CreateApp()
        {
            return new App();
        }

        protected override void FillTargetFactories(IMvxTargetBindingFactoryRegistry registry)
        {
            registry.RegisterFactory(new MvxCustomBindingFactory<MvxSwipeRefreshLayout>("RefreshCommand", (view) => new SwipeRefreshLayoutRefreshBinding(view)));
            registry.RegisterFactory(new MvxCustomBindingFactory<MvxSwipeRefreshLayout>("Refreshing", (view) => new SwipeRefreshLayoutRefreshingBinding(view)));
            registry.RegisterFactory(new MvxCustomBindingFactory<SearchView>("QueryTextChange", (view) => new SearchViewQueryTextChangeBinding(view)));
            registry.RegisterFactory(new MvxCustomBindingFactory<SearchView>("QueryTextSubmit", (view) => new SearchViewQueryTextSubmitBinding(view)));

            base.FillTargetFactories(registry);
        }

        protected override Assembly[] GetViewModelAssemblies()
        {
            return new[] { typeof(BaseViewModel).Assembly };
        }
    }
}