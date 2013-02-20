using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Opengl;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using AndroidApp.Bindings;
using AndroidApp.Core.Model.ViewModel.Dashboard;
using AndroidApp.Core.Model.ViewModel.Login;
using AndroidApp.Core.Model.ViewModel.QuestionnaireDetails;
using Cirrious.MvvmCross.Application;
using Cirrious.MvvmCross.Binding.Bindings.Target.Construction;
using Cirrious.MvvmCross.Binding.Droid.Simple;

namespace AndroidApp
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
            get { return new[] { typeof(AndroidApp.Converters.Converters) }; }
        }
        protected override void FillTargetFactories(Cirrious.MvvmCross.Binding.Interfaces.Bindings.Target.Construction.IMvxTargetBindingFactoryRegistry registry)
        {
            base.FillTargetFactories(registry);
            
            registry.RegisterFactory(new MvxCustomBindingFactory<ViewGroup>("Background", (button) => new BackgroundBinding(button)));
            registry.RegisterFactory(new MvxCustomBindingFactory<View>("Visible", (button) => new VisibilityBinding(button)));
            registry.RegisterFactory(new MvxCustomBindingFactory<TextView>("ValidationMessage", (button) => new ValidationMessageBinding(button)));
            
        }
        protected override IDictionary<Type, Type> GetViewModelViewLookup()
        {
            var lookups = base.GetViewModelViewLookup();

            if (!lookups.ContainsKey(typeof(CompleteQuestionnaireView)))
                lookups.Add(typeof(CompleteQuestionnaireView), typeof(LoadingActivity));
            if (!lookups.ContainsKey(typeof(DashboardModel)))
                lookups.Add(typeof(DashboardModel), typeof(DashboardActivity));

            if (!lookups.ContainsKey(typeof(LoginViewModel)))
                lookups.Add(typeof(LoginViewModel), typeof(LoginActivity));
            return lookups;
        }
    }
}