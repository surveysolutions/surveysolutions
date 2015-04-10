using Android.App;
using Android.Views;
using WB.Core.BoundedContexts.QuestionnaireTester.ViewModels;

namespace WB.UI.QuestionnaireTester.Views
{
    [Activity(Label = "", Theme = "@style/Theme.Blue.Light", HardwareAccelerated = true, WindowSoftInputMode = SoftInput.StateHidden)]
    public class InterviewGroupView : BaseActivityView<InterviewGroupViewModel>
    {
        protected override int ViewResourceId
        {
            get { return Resource.Layout.interview_group; }
        } 
    }
}