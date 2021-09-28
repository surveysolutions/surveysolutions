using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Android.Gms.Maps;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using AndroidX.AppCompat.Widget;
using AndroidX.DrawerLayout.Widget;
using AndroidX.RecyclerView.Widget;
using Com.Google.Android.Exoplayer2.UI;
using FFImageLoading.Cross;
using Google.Android.Material.Button;
using Google.Android.Material.TextField;
using Microsoft.Extensions.Logging;
using MvvmCross;
using MvvmCross.Binding.Bindings.Target.Construction;
using MvvmCross.Binding.Combiners;
using MvvmCross.Converters;
using MvvmCross.DroidX.RecyclerView;
using MvvmCross.IoC;
using MvvmCross.ViewModels;
using MvvmCross.Views;
using NLog.Extensions.Logging;
using Serilog;
using Serilog.Extensions.Logging;
using WB.Core.SharedKernels.Enumerator;
using WB.Core.SharedKernels.Enumerator.ViewModels;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Overview;
using WB.UI.Shared.Enumerator.Activities;
using WB.UI.Shared.Enumerator.Converters;
using WB.UI.Shared.Enumerator.CustomBindings;
using WB.UI.Shared.Enumerator.CustomControls;
using WB.UI.Shared.Enumerator.CustomControls.MaskedEditTextControl;
using WB.UI.Shared.Enumerator.ValueCombiners;
using Xamarin.Controls;
using BindingFlags = System.Reflection.BindingFlags;

namespace WB.UI.Shared.Enumerator
{
    public abstract class 
        EnumeratorSetup<TApplication> : MvvmCross.Platforms.Android.Core.MvxAndroidSetup<TApplication> 
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
            NLog.LogManager.GetCurrentClassLogger().Error(exception);
        }
        
        
        protected override IMvxViewsContainer InitializeViewLookup(IDictionary<Type, Type> viewModelViewLookup, IMvxIoCProvider iocProvider)
        {
            var lookup = base.InitializeViewLookup(viewModelViewLookup, iocProvider);
            lookup.Add<EnumerationStageViewModel, InterviewEntitiesListFragment>();
            lookup.Add<CoverInterviewViewModel, CoverInterviewFragment>();
            lookup.Add<OverviewViewModel, OverviewFragment>();
            lookup.Add<OverviewNodeDetailsViewModel, OverviewNodeDetailsFragment>();
            lookup.Add<SelectResponsibleForAssignmentViewModel, SelectResponsibleForAssignmentFragment>();
            return lookup;
        }

        protected override ILoggerProvider CreateLogProvider()
        {
            return null;// new NLogLoggerProvider();

            //return new SerilogLoggerProvider();
        }

        protected override ILoggerFactory CreateLogFactory()
        {
            //configure it
            return null; // new NLogLoggerFactory();

            // // serilog configuration
            //Log.Logger = new LoggerConfiguration()
            //    .MinimumLevel.Debug()
            //    .WriteTo.AndroidLog()
            //    .CreateLogger();

            //return new SerilogLoggerFactory();
        }
        
        
        //public override MvxLogProviderType GetDefaultLogProviderType() => MvxLogProviderType.NLog;

        protected override void FillValueConverters(IMvxValueConverterRegistry registry)
        {
            base.FillValueConverters(registry);
            
            Mvx.CallbackWhenRegistered<IMvxValueCombinerRegistry>(combinerRegistry => 
                combinerRegistry.AddOrOverwriteFrom(Assembly.GetAssembly(typeof(LayoutBackgroundStyleValueCombiner))));

            registry.AddOrOverwrite("EnumToString", new EnumToStringConverter());
            registry.AddOrOverwrite("GroupStateToColor", new GroupStateToColorConverter());
            registry.AddOrOverwrite("CommentStateToColor", new IsCurrentUserCommentToColorConverter());
            registry.AddOrOverwrite("InMemoryImageValueWithDefault", new InMemoryImageValueWithDefaultConverter());
            registry.AddOrOverwrite("QuestionLayoutStyleBackground", new QuestionLayoutStyleBackgroundConverter());
            registry.AddOrOverwrite("QuestionEditorStyleBackground", new QuestionEditorStyleBackgroundConverter());
            registry.AddOrOverwrite("QuestionCornerOptionBackground", new QuestionCornerOptionBackgroundConverter());
            registry.AddOrOverwrite("QuestionOptionBackground", new QuestionOptionBackgroundConverter());
            registry.AddOrOverwrite("IsStringNotEmpty", new IsStringNotEmptyConverter());
            registry.AddOrOverwrite("SectionStyleBackground", new SectionStyleBackgroundConverter());
            registry.AddOrOverwrite("VisibleOrInvisible", new VisibleOrInvisibleValueConverter());
            registry.AddOrOverwrite("AudioNoiseTypeToShape", new AudioNoiseTypeToShapeConverter());
            registry.AddOrOverwrite("AudioNoiseTypeToDot", new AudioNoiseTypeToDotConverter());

            registry.AddOrOverwrite("Localization", new EnumeratorLocalizationValueConverter());
            registry.AddOrOverwrite("StatusToDasboardBackground", new StatusToDasboardBackgroundConverter());
            registry.AddOrOverwrite("CalendarEventToColor", new CalendarEventToColorConverter());
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
            registry.RegisterCustomBindingFactory<TextInputLayout>("Hint", (view) => new TextInputLayoutHintBinding(view));
            registry.RegisterCustomBindingFactory<TextInputLayout>("EndIconClick", (view) => new TextInputLayoutEndIconClickBinding(view));
            registry.RegisterCustomBindingFactory<TextView>("Html", (view) => new TextViewHtmlBinding(view));
            registry.RegisterCustomBindingFactory<TextView>("TextFormatted", (view) => new TextViewTextFormattedBinding(view));
            registry.RegisterCustomBindingFactory<TextView>("IsSelectedYesNoOptionColor", (view) => new TextViewIsSelectedYesNoOptionColorBinding(view));
            registry.RegisterCustomBindingFactory<MaskedEditText>("IsMaskedQuestionAnswered", (editText) => new MaskedEditTextIsMaskedQuestionAnsweredBinding(editText));
            registry.RegisterCustomBindingFactory<EditText>("FocusValueChanged", (editText) => new EditTextFocusValueChangedBinding(editText));
            registry.RegisterCustomBindingFactory<EditText>("SetFocus", (editText) => new EditTextSetFocusBinding(editText));
            registry.RegisterCustomBindingFactory<ProgressBar>("ShowProgress", (view) => new ProgressBarIndeterminateBinding(view));
            registry.RegisterCustomBindingFactory<ProgressBar>("Progress", (view) => new ProgressBarProgressBinding(view));
            registry.RegisterCustomBindingFactory<ProgressBar>("IndeterminateMode", (view) => new ProgressBarIndeterminateModeBinding(view));
            registry.RegisterCustomBindingFactory<View>("BackgroundStyle", (view) => new ViewBackgroundDrawableBinding(view));
            registry.RegisterCustomBindingFactory<TextView>("Bold", textView => new TextViewBoldBinding(textView));
            registry.RegisterCustomBindingFactory<View>("BackgroundColor", textView => new ViewBackgroundColorBinding(textView));
            registry.RegisterCustomBindingFactory<EditText>("DateChange", editText => new EditTextDateBinding(editText));
            registry.RegisterCustomBindingFactory<Button>("ButtonGroupStyle", button => new ButtonGroupStyleBinding(button));
            registry.RegisterCustomBindingFactory<MaterialButton>("ToParentButtonGroupStyle", button => new ToParentButtonGroupStyleButtonBinding(button));
            registry.RegisterCustomBindingFactory<MaterialButton>("ButtonToQuestionState", button => new MaterialButtonToQuestionStateBinding(button));
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
            registry.RegisterCustomBindingFactory<ImageView>("QRCode", img => new ImageViewQRCodeBinding(img));
            registry.RegisterCustomBindingFactory<PlayerView>("Video", player => new ExoPlayerBinding(player));
            registry.RegisterCustomBindingFactory<PlayerView>("Audio", player => new ExoPlayerAudioAttachmentBinding(player));
            registry.RegisterCustomBindingFactory<View>("SizeByNoiseLevel", (view) => new AudioSizeByNoiseLevelBinding(view));
            registry.RegisterCustomBindingFactory<View>("Tag", (img) => new ViewTagBinding(img));
            registry.RegisterCustomBindingFactory<SignaturePadView>("Signature", (view) => new SignatureBinding(view));
            registry.RegisterCustomBindingFactory<SignaturePadView>("SignaturePadSettings", (view) => new SignaturePadSettingsBinding(view));
            registry.RegisterCustomBindingFactory<ImageButton>("Playback", (view) => new ImageButtonPlaybackToggleBinding(view));
            registry.RegisterCustomBindingFactory<RecyclerView>("ScrollToPosition", view => new RecyclerViewScrollToPositionBinding(view));
            registry.RegisterCustomBindingFactory<TextView>("HtmlWithLinks", (view) => new TextViewHtmlWithLinksBinding(view));
            registry.RegisterCustomBindingFactory<View>("ViewOverviewNodeState", (view) => new ViewOverviewNodeStateBinding(view));
            registry.RegisterCustomBindingFactory<TextView>("TextViewNodeStateTextColor", (view) => new TextViewNodeStateTextColorBinding(view));
            registry.RegisterCustomBindingFactory<TextView>("TextViewAnswerState", (view) => new TextViewAnswerStateBinding(view));
            registry.RegisterCustomBindingFactory<RadioButton>("AssignToInterviewerText", (view) => new AssignToInterviewerTextBinding(view));
            registry.RegisterCustomBindingFactory<EditText>("TextLength", (editText) => new EditTextMaxLengthBinding(editText));
            registry.RegisterCustomBindingFactory<MapView>("SetGpsLocation", view => new ViewSetGpsLocationBinding(view));
            
            //MvxAppCompatSetupHelper.FillTargetFactories(registry);

            RegisterAutoCompleteTextViewBindings(registry);
            base.FillTargetFactories(registry);
        }

        private static void RegisterAutoCompleteTextViewBindings(IMvxTargetBindingFactoryRegistry registry)
        {
            registry.RegisterCustomBindingFactory<InstantAutoCompleteTextView>("PartialText",
                (ctrl) => new InstantAutoCompleteTextViewPartialTextTargetBinding(ctrl));

            registry.RegisterCustomBindingFactory<InstantAutoCompleteTextView>("OnPartialTextChanged",
                (ctrl) => new InstantAutoCompleteTextViewOnPartialTextChangedBinding(ctrl));

            registry.RegisterCustomBindingFactory<InstantAutoCompleteTextView>("OnItemSelected",
                (ctrl) => new InstantAutoCompleteTextViewOnItemSelectedBinding(ctrl));
            
            registry.RegisterCustomBindingFactory<InstantAutoCompleteTextView>("DisableDefaultSearch",
                (ctrl) => new InstantAutoCompleteTextViewDisableDefaultSearchBinding(ctrl));

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
