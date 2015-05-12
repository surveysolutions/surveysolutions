using System;
using System.Collections.Generic;
using System.Reflection;
using Android.Content;
using Android.Support.V4.Widget;
using Android.Support.V7.Widget;
using Android.Widget;
using Cirrious.CrossCore;
using Cirrious.CrossCore.Converters;
using Cirrious.CrossCore.IoC;
using Cirrious.MvvmCross.Binding.Bindings.Target.Construction;
using Cirrious.MvvmCross.Droid.Platform;
using Cirrious.MvvmCross.ViewModels;
using Cirrious.MvvmCross.Views;
using Mapbox.MapboxSdk.Views;
using WB.Core.BoundedContexts.QuestionnaireTester.ViewModels;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities.QuestionModels;
using WB.UI.QuestionnaireTester.Converters;
using WB.UI.QuestionnaireTester.CustomBindings;
using WB.UI.QuestionnaireTester.Ninject;
using WB.UI.QuestionnaireTester.Views;
using WB.UI.QuestionnaireTester.Views.CustomControls;
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
                {typeof (LoginViewModel), typeof (LoginView)},
                {typeof (InterviewViewModel), typeof (InterviewView)},
                {typeof (DashboardViewModel), typeof (DashboardView)},
                {typeof (PrefilledQuestionsViewModel), typeof (PrefilledQuestionsView)},
                {typeof (SearchQuestionnairesViewModel), typeof (SearchQuestionnairesView)}
            };

            var container = Mvx.Resolve<IMvxViewsContainer>();
            container.AddAll(viewModelViewLookup);
        }

        protected override void FillValueConverters(IMvxValueConverterRegistry registry)
        {
            base.FillValueConverters(registry);

            registry.AddOrOverwrite("Localization", new LocalizationValueConverter());
        }

        protected override void FillTargetFactories(IMvxTargetBindingFactoryRegistry registry)
        {
            registry.RegisterFactory(new MvxCustomBindingFactory<TextView>("Hint", (view) => new TextViewHintBinding(view)));
            registry.RegisterFactory(new MvxCustomBindingFactory<EditText>("Mask", (editText) => new EditTextMaskBinding(editText)));
            registry.RegisterFactory(new MvxCustomBindingFactory<EditText>("ValueChange", (editText) => new EditTextValueChangedBinding(editText)));
            registry.RegisterFactory(new MvxCustomBindingFactory<ProgressBar>("ShowProgress", (view) => new ProgressBarIndeterminateBinding(view)));

            registry.RegisterCustomBindingFactory<TextView>("Bold", textView => new TextViewBoldBinding(textView));

            base.FillTargetFactories(registry);
        }

        protected override IDictionary<string, string> ViewNamespaceAbbreviations
        {
            get
            {
                var namespaceAbbreviations = base.ViewNamespaceAbbreviations;
                namespaceAbbreviations["Wb"] = "WB.UI.QuestionnaireTester.Controls";
                return namespaceAbbreviations;
            }
        }

        protected override IList<Assembly> AndroidViewAssemblies
        {
            get
            {
                var toReturn = base.AndroidViewAssemblies;

                // Add assemblies with other views we use.  When the XML is inflated
                // MvvmCross knows about the types and won't compain about them.  This
                // speeds up inflation noticeably.
                toReturn.Add(typeof(MvxRecyclerView).Assembly);
                toReturn.Add(typeof(DrawerLayout).Assembly);
                toReturn.Add(typeof(SwitchCompat).Assembly);
                toReturn.Add(typeof(MapView).Assembly);
                return toReturn;
            }
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