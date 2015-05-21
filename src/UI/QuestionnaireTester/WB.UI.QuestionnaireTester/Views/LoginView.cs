using Android.App;
using Android.Views;
using WB.Core.BoundedContexts.QuestionnaireTester.ViewModels;

namespace WB.UI.QuestionnaireTester.Views
{
    [Activity(NoHistory = true, WindowSoftInputMode = SoftInput.StateHidden, Theme = "@style/GrayAppTheme")]
    public class LoginView : BaseActivityView<LoginViewModel>
    {
        protected override int ViewResourceId
        {
            get { return Resource.Layout.login; }
        }
    }
}