using System;
using System.Collections.Generic;
using Android.Content;
using Android.Views;
using Android.Widget;
using CAPI.Android.Bindings;
using CAPI.Android.Core.Model.ViewModel.Dashboard;
using CAPI.Android.Core.Model.ViewModel.Login;
using CAPI.Android.Core.Model.ViewModel.QuestionnaireDetails;
using Cirrious.MvvmCross.Application;
using Cirrious.MvvmCross.Binding.Bindings.Target.Construction;
using Cirrious.MvvmCross.Binding.Droid.Simple;

namespace CAPI.Android
{
    public class Setup
       : MvxSimpleAndroidBindingSetup
    {
        public Setup(Context applicationContext)
            : base(applicationContext)
        {
        }
        protected override MvxApplication CreateApp()
        {
            return new App();
        }
        protected override IEnumerable<Type> ValueConverterHolders
        {
            get { return new[] { typeof(Converters.Converters) }; }
        }
        protected override void FillTargetFactories(Cirrious.MvvmCross.Binding.Interfaces.Bindings.Target.Construction.IMvxTargetBindingFactoryRegistry registry)
        {
            base.FillTargetFactories(registry);
            
            registry.RegisterFactory(new MvxCustomBindingFactory<ViewGroup>("Background", (button) => new BackgroundBinding(button)));
            registry.RegisterFactory(new MvxCustomBindingFactory<TextView>("Html", (button) => new HtmlBinding(button)));
            registry.RegisterFactory(new MvxCustomBindingFactory<View>("Visible", (button) => new VisibilityBinding(button)));
            registry.RegisterFactory(new MvxCustomBindingFactory<TextView>("ValidationMessage", (button) => new ValidationMessageBinding(button)));
            
        }
        protected override IDictionary<Type, Type> GetViewModelViewLookup()
        {
            var lookups = base.GetViewModelViewLookup();

        /*    if (!lookups.ContainsKey(typeof(CompleteQuestionnaireView)))
                lookups.Add(typeof(CompleteQuestionnaireView), typeof(LoadingActivity));
            if (!lookups.ContainsKey(typeof(DashboardModel)))
                lookups.Add(typeof(DashboardModel), typeof(DashboardActivity));*/

            if (!lookups.ContainsKey(typeof(LoginViewModel)))
                lookups.Add(typeof(LoginViewModel), typeof(LoginActivity));
            return lookups;
        }
    }
}