using Android.App;
using Android.Views;
using Cirrious.MvvmCross.Binding.Droid.BindingContext;
using Cirrious.MvvmCross.Binding.Droid.Views;
using WB.Core.BoundedContexts.QuestionnaireTester.ViewModels;
using Android.OS;
using WB.UI.QuestionnaireTester.Views.Adapters;


namespace WB.UI.QuestionnaireTester.Views
{
    [Activity(Label = "", Theme = "@style/Theme.Blue.Light", HardwareAccelerated = true, WindowSoftInputMode = SoftInput.StateHidden)]
    public class InterviewGroupView : BaseActivityView<InterviewGroupViewModel>
    {
        protected override int ViewResourceId
        {
            get { return Resource.Layout.interview_group; }
        }

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            var list = FindViewById<MvxListView>(Resource.Id.QuestionsListView);
            list.Adapter = new QuestionAdapter(this, (IMvxAndroidBindingContext)BindingContext);
        }
    }
}