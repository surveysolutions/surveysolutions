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
using Cirrious.MvvmCross.Binding.Bindings.Target.Construction;
using Cirrious.MvvmCross.Binding.Combiners;
using Cirrious.MvvmCross.Binding.Droid.Views;
using Cirrious.MvvmCross.ViewModels;
using Cirrious.MvvmCross.Views;
using WB.Core.BoundedContexts.Capi.Views.Login;
using WB.Core.BoundedContexts.Tester;
using WB.Core.BoundedContexts.Tester.ViewModels;
using WB.UI.Capi.Implementations.Activities;
using WB.UI.Capi.Injections;
using WB.UI.Shared.Android;
using WB.UI.Tester.Activities;
using WB.UI.Tester.Converters;
using WB.UI.Tester.CustomBindings;
using WB.UI.Tester.CustomControls;
using WB.UI.Tester.CustomControls.MaskedEditTextControl;
using WB.UI.Tester.Ninject;
using WB.UI.Tester.ValueCombiners;
using FinishIntallationViewModel = WB.UI.Capi.Views.FinishIntallationViewModel;
using LoginViewModel = WB.Core.BoundedContexts.Capi.Views.Login.LoginViewModel;

namespace WB.UI.Capi
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
            container.Add(typeof(FinishIntallationViewModel), typeof(FinishInstallationActivity));
            container.Add(typeof(InterviewViewModel), typeof(InterviewActivity));
        }

        protected override IMvxApplication CreateApp()
        {
            return new App();
        }

        protected override IMvxIoCProvider CreateIocProvider()
        {
            return new NinjectMvxIocProvider(CapiApplication.Kernel);
        }

        protected override Assembly[] GetViewModelAssemblies()
        {
            return new[]
            {
                typeof(AndroidCoreRegistry).Assembly,
                typeof(EnumeratorSharedKernelModule).Assembly,
            };
        }

        protected override void FillValueConverters(IMvxValueConverterRegistry registry)
        {
            base.FillValueConverters(registry);

            Mvx.CallbackWhenRegistered<IMvxValueCombinerRegistry>(combinerRegistry =>
                combinerRegistry.AddOrOverwriteFrom(Assembly.GetAssembly(typeof(LayoutBackgroundStyleValueCombiner))));

            registry.AddOrOverwrite("Localization", new LocalizationValueConverter());
            registry.AddOrOverwrite("GroupStateToColor", new GroupStateToColorConverter());
            registry.AddOrOverwrite("ByteArrayToImage", new ByteArrayToImageConverter());
            registry.AddOrOverwrite("ToGoogleMapUrl", new ToGoogleMapUrlConverter());
            registry.AddOrOverwrite("QuestionLayoutStyleBackground", new QuestionLayoutStyleBackgroundConverter());
            registry.AddOrOverwrite("QuestionEditorStyleBackground", new QuestionEditorStyleBackgroundConverter());
            registry.AddOrOverwrite("MediaButtonStyleBackground", new MediaQuestionButtonBackgroundConverter());
            registry.AddOrOverwrite("ViewOptionStyleBackground", new ViewOptionStyleBackgroundConverter());
            registry.AddOrOverwrite("SectionStyleBackground", new SectionStyleBackgroundConverter());
            registry.AddOrOverwrite("PublicBackground", new QuestionnairePublicityBackgroundConverter());
        }

        protected override void FillTargetFactories(IMvxTargetBindingFactoryRegistry registry)
        {
            registry.RegisterCustomBindingFactory<TextView>("Hint", (view) => new TextViewHintBinding(view));
            registry.RegisterCustomBindingFactory<TextView>("Html", (view) => new TextViewHtmlBinding(view));
            registry.RegisterCustomBindingFactory<TextView>("TextFormatted", (view) => new TextViewTextFormattedBinding(view));
            registry.RegisterCustomBindingFactory<TextView>("SideBarPrefillQuestion", (view) => new TextViewSideBarPrefillQuestionBinding(view));
            registry.RegisterCustomBindingFactory<MaskedEditText>("IsMaskedQuestionAnswered", (editText) => new MaskedEditTextIsMaskedQuestionAnsweredBinding(editText));
            registry.RegisterCustomBindingFactory<EditText>("FocusValueChanged", (editText) => new EditTextFocusValueChangedBinding(editText));
            registry.RegisterCustomBindingFactory<EditText>("SetFocus", (editText) => new EditTextSetFocusBinding(editText));
            registry.RegisterCustomBindingFactory<ProgressBar>("ShowProgress", (view) => new ProgressBarIndeterminateBinding(view));
            registry.RegisterCustomBindingFactory<View>("BackgroundStyle", (view) => new ViewBackgroundDrawableBinding(view));
            registry.RegisterCustomBindingFactory<TextView>("Bold", textView => new TextViewBoldBinding(textView));
            registry.RegisterCustomBindingFactory<EditText>("DecimalPlaces", editText => new EditTextDecimalPlacesBinding(editText));
            registry.RegisterCustomBindingFactory<EditText>("DateChange", editText => new EditTextDateBinding(editText));
            registry.RegisterCustomBindingFactory<TextView>("GroupInfo", textView => new TextViewGroupInfoBinding(textView));
            registry.RegisterCustomBindingFactory<Button>("ButtonGroupStyle", button => new ButtonGroupStyleBinding(button));
            registry.RegisterCustomBindingFactory<TextView>("GroupStatus", textView => new TextViewGroupStatusBinding(textView));
            registry.RegisterCustomBindingFactory<View>("HideKeyboardOnClick", view => new ViewHideKeyboardOnClickBinding(view));
            registry.RegisterCustomBindingFactory<MvxAutoCompleteTextView>("HidePopupOnDone", view => new MvxAutoCompleteTextViewHidePopupOnDoneBinding(view));
            registry.RegisterCustomBindingFactory<MvxAutoCompleteTextView>("ResetText", view => new MvxAutoCompleteTextViewResetTextBinding(view));
            registry.RegisterCustomBindingFactory<MvxAutoCompleteTextView>("ShowPopupOnFocus", view => new MvxAutoCompleteTextViewShowPopupOnFocusBinding(view));
            registry.RegisterCustomBindingFactory<ViewGroup>("ColorByInterviewStatus", view => new ViewGroupColorByInterviewStatusBinding(view));
            registry.RegisterCustomBindingFactory<ViewGroup>("StatusBarColorByInterviewStatus", view => new ViewGroupStatusBarColorByInterviewStatusBinding(view));
            registry.RegisterCustomBindingFactory<View>("Transparent", view => new ViewTransparentBinding(view));
            registry.RegisterCustomBindingFactory<View>("PaddingLeft", view => new ViewPaddingLeftBinding(view));
            registry.RegisterCustomBindingFactory<View>("Activated", view => new ViewActivatedBinding(view));

            base.FillTargetFactories(registry);
        }

        protected override IDictionary<string, string> ViewNamespaceAbbreviations
        {
            get
            {
                var namespaceAbbreviations = base.ViewNamespaceAbbreviations;
                namespaceAbbreviations["Wb"] = "WB.UI.Tester.Controls";
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
    }
}