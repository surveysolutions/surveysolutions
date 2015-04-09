using Android.App;
using Android.OS;
using Android.Support.V7.Widget;
using Android.Views;

using WB.Core.BoundedContexts.QuestionnaireTester.ViewModels;

namespace WB.UI.QuestionnaireTester.Views
{
    [Activity(Label = "", Theme = "@style/Theme.Gray.Light", HardwareAccelerated = true, WindowSoftInputMode = SoftInput.StateHidden)]
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
        }
    }
}