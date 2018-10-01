using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Android.Runtime;
using Android.Support.V4.Widget;
using Android.Support.V7.Widget;
using Android.Views;
using Android.Widget;
using FFImageLoading.Cross;
using MvvmCross;
using MvvmCross.Binding.Bindings.Target.Construction;
using MvvmCross.Binding.Combiners;
using MvvmCross.Converters;
using MvvmCross.Droid.Support.V7.AppCompat;
using MvvmCross.Droid.Support.V7.RecyclerView;
using MvvmCross.Logging;
using MvvmCross.Platforms.Android.Presenters;
using MvvmCross.ViewModels;
using MvvmCross.Views;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.SharedKernels.Enumerator;
using WB.Core.SharedKernels.Enumerator.ViewModels;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Overview;
using WB.UI.Shared.Enumerator.Activities;
using WB.UI.Shared.Enumerator.Converters;
using WB.UI.Shared.Enumerator.CustomBindings;
using WB.UI.Shared.Enumerator.CustomControls;
using WB.UI.Shared.Enumerator.CustomControls.MaskedEditTextControl;
using WB.UI.Shared.Enumerator.CustomServices;
using WB.UI.Shared.Enumerator.ValueCombiners;
using Xamarin.Controls;
using BindingFlags = System.Reflection.BindingFlags;

namespace WB.UI.Shared.Enumerator
{
    public abstract class 
        EnumeratorSetup<TApplication> : MvxAppCompatSetup<TApplication> 
        where TApplication : class, IMvxApplication, new()
    {
        protected EnumeratorSetup()
        {
            //restart the app to avoid incorrect state
            TaskScheduler.UnobservedTaskException += (sender, args) =>
            {
                ProcessException(args.Exception);
            };
            AndroidEnvironment.UnhandledExceptionRaiser += (sender, args) =>
            {
                var exception = args.Exception;

                // this is super dirty hack in order to get exception's stack trace which happend inside async method
                FieldInfo stackTrace = typeof(Exception).GetField("stack_trace", BindingFlags.NonPublic | BindingFlags.Instance);
                stackTrace?.SetValue(exception, null);
                this.ProcessException(Java.Lang.Throwable.FromException(exception));

                ProcessException(args.Exception);
            };
            AppDomain.CurrentDomain.UnhandledException += (sender, args) =>
            {
                object exceptionObject = args.ExceptionObject;

                if (exceptionObject is Exception typedException)
                {
                    ProcessException(typedException);
                }
                else
                {
                    ProcessException(new Exception("Untyped exception message: '" + exceptionObject + "'"));
                }
            };
        }

        protected virtual void ProcessException(Exception exception)
        {
            Mvx.Resolve<IMvxLogProvider>().GetLogFor("EnumeratorSetup").Error(exception, "UncaughtExceptionHandler");
            Mvx.Resolve<ILogger>().Fatal("UncaughtExceptionHandler", exception);
        }

        protected override void InitializeViewLookup()
        {
            base.InitializeViewLookup();
            var viewModelViewLookup = new Dictionary<Type, Type>()
            {
                {typeof (EnumerationStageViewModel), typeof (InterviewEntitiesListFragment)},
                {typeof(CoverInterviewViewModel), typeof (CoverInterviewFragment)},
                {typeof(OverviewViewModel), typeof (OverviewFragment)},
                {typeof(OverviewNodeDetailsViewModel), typeof(OverviewNodeDetailsFragment)},
                {typeof(SelectResponsibleForAssignmentViewModel), typeof(SelectResponsibleForAssignmentFragment)},
            };

            var container = Mvx.Resolve<IMvxViewsContainer>();
            container.AddAll(viewModelViewLookup);
        }

        protected override void FillValueConverters(IMvxValueConverterRegistry registry)
        {
            base.FillValueConverters(registry);
            
            Mvx.CallbackWhenRegistered<IMvxValueCombinerRegistry>(combinerRegistry => 
                combinerRegistry.AddOrOverwriteFrom(Assembly.GetAssembly(typeof(LayoutBackgroundStyleValueCombiner))));

            registry.AddOrOverwrite("EnumToString", new EnumToStringConverter());
            registry.AddOrOverwrite("GroupStateToColor", new GroupStateToColorConverter());
            registry.AddOrOverwrite("IsCurrentUserCommentToColor", new IsCurrentUserCommentToColorConverter());
            registry.AddOrOverwrite("ToGoogleMapUrl", new ToGoogleMapUrlConverter());
            registry.AddOrOverwrite("InMemoryImageValueWithDefault", new InMemoryImageValueWithDefaultConverter());
            registry.AddOrOverwrite("QuestionLayoutStyleBackground", new QuestionLayoutStyleBackgroundConverter());
            registry.AddOrOverwrite("QuestionEditorStyleBackground", new QuestionEditorStyleBackgroundConverter());
            registry.AddOrOverwrite("QuestionCornerOptionBackground", new QuestionCornerOptionBackgroundConverter());
            registry.AddOrOverwrite("QuestionOptionBackground", new QuestionOptionBackgroundConverter());
            registry.AddOrOverwrite("IsStringNotEmpty", new IsStringNotEmptyConverter());
            registry.AddOrOverwrite("MediaButtonStyleBackground", new MediaQuestionButtonBackgroundConverter());
            registry.AddOrOverwrite("SectionStyleBackground", new SectionStyleBackgroundConverter());
            registry.AddOrOverwrite("VisibleOrInvisible", new VisibleOrInvisibleValueConverter());
            registry.AddOrOverwrite("AudioNoiseTypeToShape", new AudioNoiseTypeToShapeConverter());
            registry.AddOrOverwrite("AudioNoiseTypeToDot", new AudioNoiseTypeToDotConverter());

            registry.AddOrOverwrite("Localization", new EnumeratorLocalizationValueConverter());
            registry.AddOrOverwrite("StatusToDasboardBackground", new StatusToDasboardBackgroundConverter());
            registry.AddOrOverwrite("InterviewStatusToColor", new InterviewStatusToColorConverter());
            registry.AddOrOverwrite("InterviewStatusToDrawable", new InterviewStatusToDrawableConverter());
            registry.AddOrOverwrite("InterviewStatusToButton", new InterviewStatusToButtonConverter());
            registry.AddOrOverwrite("SynchronizationStatusToDrawable", new SynchronizationStatusToDrawableConverter());
            registry.AddOrOverwrite("ValidationStyleBackground", new TextEditValidationStyleBackgroundConverter());
            registry.AddOrOverwrite("IsSynchronizationFailOrCanceled", new IsSynchronizationFailOrCanceledConverter());
            registry.AddOrOverwrite("SynchronizationStatusToTextColor", new SynchronizationStatusToTextColorConverter());
        }

        protected override void FillTargetFactories(IMvxTargetBindingFactoryRegistry registry)
        {
            registry.RegisterCustomBindingFactory<NumericEditText>("Value", (view) => new NumericValueBinding(view));
            registry.RegisterCustomBindingFactory<NumericEditText>("Disabled", (view) => new NumericDisableBinding(view));
            registry.RegisterCustomBindingFactory<TextView>("Watermark", (view) => new TextViewWatermarkBinding(view));
            registry.RegisterCustomBindingFactory<TextView>("Html", (view) => new TextViewHtmlBinding(view));
            registry.RegisterCustomBindingFactory<TextView>("TextFormatted", (view) => new TextViewTextFormattedBinding(view));
            registry.RegisterCustomBindingFactory<TextView>("IsSelectedYesNoOptionColor", (view) => new TextViewIsSelectedYesNoOptionColorBinding(view));
            registry.RegisterCustomBindingFactory<MaskedEditText>("IsMaskedQuestionAnswered", (editText) => new MaskedEditTextIsMaskedQuestionAnsweredBinding(editText));
            registry.RegisterCustomBindingFactory<EditText>("FocusValueChanged", (editText) => new EditTextFocusValueChangedBinding(editText));
            registry.RegisterCustomBindingFactory<EditText>("SetFocus", (editText) => new EditTextSetFocusBinding(editText));
            registry.RegisterCustomBindingFactory<ProgressBar>("ShowProgress", (view) => new ProgressBarIndeterminateBinding(view));
            registry.RegisterCustomBindingFactory<ProgressBar>("Progress", (view) => new ProgressBarProgressBinding(view));
            registry.RegisterCustomBindingFactory<View>("BackgroundStyle", (view) => new ViewBackgroundDrawableBinding(view));
            registry.RegisterCustomBindingFactory<TextView>("Bold", textView => new TextViewBoldBinding(textView));
            registry.RegisterCustomBindingFactory<EditText>("DateChange", editText => new EditTextDateBinding(editText));
            registry.RegisterCustomBindingFactory<TextView>("GroupInfo", textView => new TextViewGroupInfoBinding(textView));
            registry.RegisterCustomBindingFactory<Button>("ButtonGroupStyle", button => new ButtonGroupStyleBinding(button));
            registry.RegisterCustomBindingFactory<Button>("ToParentButtonGroupStyle", button => new ToParentGroupButtonBinding(button));
            registry.RegisterCustomBindingFactory<TextView>("GroupStatus", textView => new TextViewGroupStatusBinding(textView));
            registry.RegisterCustomBindingFactory<View>("HideKeyboardOnClick", view => new ViewHideKeyboardOnClickBinding(view));
            registry.RegisterCustomBindingFactory<TextView>("OnDone", view => new TextViewOnDoneBinding(view));
            registry.RegisterCustomBindingFactory<ViewGroup>("ColorByInterviewStatus", view => new ViewGroupColorByInterviewStatusBinding(view));
            registry.RegisterCustomBindingFactory<ViewGroup>("StatusBarColorByInterviewStatus", view => new ViewGroupStatusBarColorByInterviewStatusBinding(view));
            registry.RegisterCustomBindingFactory<View>("Transparent", view => new ViewTransparentBinding(view));
            registry.RegisterCustomBindingFactory<View>("PaddingLeft", view => new ViewPaddingLeftBinding(view));
            registry.RegisterCustomBindingFactory<View>("Activated", view => new ViewActivatedBinding(view));
            registry.RegisterCustomBindingFactory<TextView>("TextColor", (view) => new TextViewTextColorBinding(view));
            registry.RegisterCustomBindingFactory<TextView>("UnderlinePressed", (view) => new TextViewUnderlinePressedBinding(view));
            registry.RegisterCustomBindingFactory<EditText>("TextChanged", (editText) => new EditTextChangedBinding(editText));
            registry.RegisterCustomBindingFactory<FrameLayout>("CurrentScreen", (frameLayout) => new FrameLayoutCurrentScreenBinding(frameLayout));
            registry.RegisterCustomBindingFactory<ImageView>("BitmapWithFallback", (img) => new ImageViewBitmapWithFallbackBinding(img));
            registry.RegisterCustomBindingFactory<View>("SizeByNoiseLevel", (view) => new AudioSizeByNoiseLevelBinding(view));
            registry.RegisterCustomBindingFactory<View>("Tag", (img) => new ViewTagBinding(img));
            registry.RegisterCustomBindingFactory<SignaturePadView>("Signature", (view) => new SignatureBinding(view));
            registry.RegisterCustomBindingFactory<SignaturePadView>("SignaturePadSettings", (view) => new SignaturePadSettingsBinding(view));
            registry.RegisterCustomBindingFactory<ImageButton>("Playback", (view) => new ImageButtonPlaybackToggleBinding(view));
            registry.RegisterCustomBindingFactory<RecyclerView>("ScrollToPosition", view => new RecyclerViewScrollToPositionBinding(view));

            registry.RegisterCustomBindingFactory<View>("ViewOverviewNodeState", (view) => new ViewOverviewNodeStateBinding(view));
            registry.RegisterCustomBindingFactory<TextView>("TextViewNodeStateTextColor", (view) => new TextViewNodeStateTextColorBinding(view));
            registry.RegisterCustomBindingFactory<TextView>("TextViewAnswerState", (view) => new TextViewAnswerStateBinding(view));
            registry.RegisterCustomBindingFactory<RadioButton>("AssignToInterviewerText", (view) => new AssignToInterviewerTextBinding(view));
            MvxAppCompatSetupHelper.FillTargetFactories(registry);

            RegisterAutoCompleteTextViewBindings(registry);
            base.FillTargetFactories(registry);
        }

        private static void RegisterAutoCompleteTextViewBindings(IMvxTargetBindingFactoryRegistry registry)
        {
            registry.RegisterCustomBindingFactory<InstantAutoCompleteTextView>(nameof(InstantAutoCompleteTextView.PartialText),
                (ctrl) => new InstantAutoCompleteTextViewPartialTextTargetBinding(ctrl));

            registry.RegisterCustomBindingFactory<InstantAutoCompleteTextView>("OnPartialTextChanged",
                (ctrl) => new InstantAutoCompleteTextViewOnPartialTextChangedBinding(ctrl));

            registry.RegisterCustomBindingFactory<InstantAutoCompleteTextView>("OnItemSelected",
                (ctrl) => new InstantAutoCompleteTextViewOnItemSelectedBinding(ctrl));

            registry.RegisterCustomBindingFactory<InstantAutoCompleteTextView>("OnFocusOut",
                (ctrl) => new InstantAutoCompleteTextViewOnFocusOutBinding(ctrl));
        }

        protected override IEnumerable<Assembly> AndroidViewAssemblies =>
            base.AndroidViewAssemblies.Union(new[]
            {
                typeof (FlowLayout).Assembly,
                typeof (MvxRecyclerView).Assembly,
                typeof (DrawerLayout).Assembly,
                typeof (SwitchCompat).Assembly,
                typeof (MvxCachedImageView).Assembly
            });

        public override IEnumerable<Assembly> GetViewModelAssemblies()
        {
            return new[]
            {
                typeof(EnumeratorSharedKernelModule).Assembly,
                typeof(EnumeratorUIModule).Assembly,
            };
        }
    }
}
