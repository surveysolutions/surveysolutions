using System;
using System.Collections.Generic;
using System.Reflection;
using Android.Content;
using Android.Support.V4.Widget;
using Android.Support.V7.Widget;
using Android.Views;
using Android.Widget;
using Cirrious.CrossCore;
using Cirrious.CrossCore.Converters;
using Cirrious.CrossCore.IoC;
using Cirrious.MvvmCross.Binding.Binders;
using Cirrious.MvvmCross.Binding.Bindings.Target.Construction;
using Cirrious.MvvmCross.Binding.Combiners;
using Cirrious.MvvmCross.Droid.Platform;
using Cirrious.MvvmCross.ViewModels;
using Cirrious.MvvmCross.Views;
using WB.Core.BoundedContexts.QuestionnaireTester.ViewModels;
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
            
            Mvx.CallbackWhenRegistered<IMvxValueCombinerRegistry>(combinerRegistry => 
                combinerRegistry.AddOrOverwriteFrom(Assembly.GetAssembly(typeof(Setup))));

            registry.AddOrOverwrite("Localization", new LocalizationValueConverter());
            registry.AddOrOverwrite("GetAnswer", new GetAnswerConverter());
            registry.AddOrOverwrite("ByteArrayToImage", new ByteArrayToImageConverter());
            registry.AddOrOverwrite("ToGoogleMapUrl", new ToGoogleMapUrlConverter());
            registry.AddOrOverwrite("QuestionLayoutStyleBackground", new QuestionLayoutStyleBackgroundConverter());
            registry.AddOrOverwrite("QuestionEditorStyleBackground", new QuestionEditorStyleBackgroundConverter());
            registry.AddOrOverwrite("GetGroupInfo", new GetGroupInfoConverter());
            registry.AddOrOverwrite("GetGroupInfoTextColorByStatus", new GetGroupInfoTextColorByStatusConverter());
            registry.AddOrOverwrite("GetGroupColorByStatus", new GetGroupColorByStatusConverter());
        }

        protected override void FillTargetFactories(IMvxTargetBindingFactoryRegistry registry)
        {
            registry.RegisterCustomBindingFactory<TextView>("Hint", (view) => new TextViewHintBinding(view));
            registry.RegisterCustomBindingFactory<EditText>("Mask", (editText) => new EditTextMaskBinding(editText));
            registry.RegisterCustomBindingFactory<EditText>("FocusText", (editText) => new FocusTextForEditTextBinding(editText));
            registry.RegisterCustomBindingFactory<EditText>("SetFocus", (editText) => new SetFocusForEditTextBinding(editText));
            registry.RegisterCustomBindingFactory<ProgressBar>("ShowProgress", (view) => new ProgressBarIndeterminateBinding(view));
            registry.RegisterCustomBindingFactory<View>("BackgroundStyle", (view) => new BackgroundDrawableBinding(view));
            registry.RegisterCustomBindingFactory<TextView>("Bold", textView => new TextViewBoldBinding(textView));
            registry.RegisterCustomBindingFactory<EditText>("DateChange", editText => new EditDateBinding(editText));
            registry.RegisterCustomBindingFactory<Button>("RosterTitle", button => new ButtonRosterTitleBinding(button));

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