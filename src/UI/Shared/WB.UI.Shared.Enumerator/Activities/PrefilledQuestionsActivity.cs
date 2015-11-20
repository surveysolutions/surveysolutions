using Android.App;
using Android.OS;
using Android.Support.V7.Widget;
using Android.Views;
using Cirrious.MvvmCross.Binding.Droid.BindingContext;
using Cirrious.MvvmCross.Droid.Support.RecyclerView;
using WB.Core.SharedKernels.Enumerator.ViewModels;
using WB.UI.Shared.Enumerator.CustomControls;

namespace WB.UI.Shared.Enumerator.Activities
{
    [Activity(Label = "", Theme = "@style/BlueAppTheme", 
        HardwareAccelerated = true,
        WindowSoftInputMode = SoftInput.StateAlwaysHidden | SoftInput.AdjustPan,
        ConfigurationChanges = Android.Content.PM.ConfigChanges.Orientation | Android.Content.PM.ConfigChanges.ScreenSize)]
    public class PrefilledQuestionsActivity : BaseActivity<PrefilledQuestionsViewModel>
    {
        protected override int ViewResourceId
        {
            get { return Resource.Layout.prefilled_questions; }
        }
        public async override void OnBackPressed()
        {
            await this.ViewModel.NavigateToPreviousViewModelAsync();
        }

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            this.SetSupportActionBar(this.FindViewById<Toolbar>(Resource.Id.toolbar));

            var listOfInterviewQuestionsAndGroups = this.FindViewById<MvxRecyclerView>(Resource.Id.questionnaireEntitiesList);
            var layoutManager = new LinearLayoutManager(this);
            
            listOfInterviewQuestionsAndGroups.SetLayoutManager(layoutManager);
            listOfInterviewQuestionsAndGroups.HasFixedSize = true;
            listOfInterviewQuestionsAndGroups.Adapter = new InterviewEntityAdapter((IMvxAndroidBindingContext)this.BindingContext);
        }
    }
}