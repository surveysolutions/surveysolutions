using Android.OS;
using Android.Support.V7.App;
using Android.Support.V7.Widget;
using Android.Views;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails;

namespace WB.UI.Shared.Enumerator.Activities
{
    public abstract class SingleInterviewActivity<TViewModel> : BaseActivity<TViewModel>
        where TViewModel : SingleInterviewViewModel
    {
        protected Toolbar toolbar;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            this.toolbar = this.FindViewById<Toolbar>(Resource.Id.toolbar);
            this.SetSupportActionBar(this.toolbar);
        }

        protected void PopulateLanguagesMenu(IMenu menu, int languagesMenuId, int originalLanguageMenuItemId, int languagesMenuGroupId)
        {
            if (!this.ViewModel.IsSuccessfullyLoaded)
                return;

            ISubMenu languagesMenu = menu.FindItem(languagesMenuId).SubMenu;

            IMenuItem currentLanguageMenuItem = menu.FindItem(originalLanguageMenuItemId);

            foreach (var language in this.ViewModel.AvailableLanguages)
            {
                var languageMenuItem = languagesMenu.Add(
                    groupId: languagesMenuGroupId,
                    itemId: Menu.None,
                    order: Menu.None,
                    title: language);

                if (language == this.ViewModel.CurrentLanguage)
                {
                    currentLanguageMenuItem = languageMenuItem;
                }
            }

            languagesMenu.SetGroupCheckable(languagesMenuGroupId, checkable: true, exclusive: true);

            currentLanguageMenuItem.SetChecked(true);
        }
    }
}