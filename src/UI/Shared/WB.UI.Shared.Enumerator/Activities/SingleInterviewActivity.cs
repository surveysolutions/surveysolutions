using Android.Support.V7.App;
using Android.Views;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails;

namespace WB.UI.Shared.Enumerator.Activities
{
    public abstract class SingleInterviewActivity<TViewModel> : BaseActivity<TViewModel>
        where TViewModel : SingleInterviewViewModel
    {
        protected abstract ActionBarDrawerToggle DrawerToggle { get; }

        protected abstract void OnMenuItemSelected(IMenuItem item);

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            if (this.DrawerToggle.OnOptionsItemSelected(item))
            {
                return true;
            }

            this.OnMenuItemSelected(item);

            return base.OnOptionsItemSelected(item);
        }

        protected void PopulateLanguagesMenu(IMenu menu, int languagesMenuId, int originalLanguageMenuItemId, int languagesMenuGroupId)
        {
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