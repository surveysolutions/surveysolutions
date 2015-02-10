using Android.App;
using Android.Views;
using WB.Core.BoundedContexts.QuestionnaireTester.ViewModels;

namespace WB.UI.QuestionnaireTester.Mvvm.Views
{
    [Activity(NoHistory = true, Theme = "@style/Theme.Tester", WindowSoftInputMode = SoftInput.StateHidden)]
    public class LoginActivityView : BaseActivityView<LoginViewModel>
    {
        protected override int ViewResourceId
        {
            get { return Resource.Layout.Login; }
        }
    }
}