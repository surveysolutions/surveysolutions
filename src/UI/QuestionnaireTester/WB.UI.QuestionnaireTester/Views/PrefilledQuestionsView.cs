using Android.App;
using Android.OS;
using Android.Support.V7.Widget;
using Android.Views;
using Cirrious.MvvmCross.Binding.Droid.BindingContext;
using Cirrious.MvvmCross.Binding.Droid.Views;

using WB.Core.BoundedContexts.QuestionnaireTester.ViewModels;
using WB.UI.QuestionnaireTester.Views.Adapters;
using WB.UI.QuestionnaireTester.Views.CustomControls;

namespace WB.UI.QuestionnaireTester.Views
{
    [Activity(Label = "", Theme = "@style/Theme.Blue.Light", HardwareAccelerated = true, WindowSoftInputMode = SoftInput.StateHidden)]
    public class PrefilledQuestionsView : BaseActivityView<PrefilledQuestionsViewModel>
    {
        protected override int ViewResourceId
        {
            get { return Resource.Layout.prefilled_questions; }
        }

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            this.SetSupportActionBar(this.FindViewById<Toolbar>(Resource.Id.toolbar));

            var listOfInterviewQuestionsAndGroups = FindViewById<MvxRecyclerView>(Resource.Id.questionnaireEntitiesList);
            var layoutManager = new LinearLayoutManager(this);
            
            listOfInterviewQuestionsAndGroups.SetLayoutManager(layoutManager);
            listOfInterviewQuestionsAndGroups.HasFixedSize = true;
            listOfInterviewQuestionsAndGroups.Adapter = new InterviewEntityAdapter(this, (IMvxAndroidBindingContext)BindingContext);
        }
    }
}