using System.Collections;
using Android.Views;
using MvvmCross;
using MvvmCross.Commands;
using MvvmCross.Plugin.Messenger;
using WB.Core.SharedKernels.Enumerator.Properties;
using WB.Core.SharedKernels.Enumerator.Utils;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails;
using Toolbar = AndroidX.AppCompat.Widget.Toolbar;

namespace WB.UI.Shared.Enumerator.Activities
{
    public abstract class SingleInterviewActivity<TViewModel> : BaseActivity<TViewModel> where TViewModel : SingleInterviewViewModel
    {
        private IMvxMessenger messenger => Mvx.IoCProvider.GetSingleton<IMvxMessenger>();
        private MvxSubscriptionToken answerAcceptedSubscription;

        #region Subclasses

        public class MenuItemDescription
        {
            public MenuItemDescription(int id, string localizedTitle, IMvxCommand command = null)
            {
                this.Id = id;
                this.LocalizedTitle = localizedTitle;
                this.Command = command;
            }

            public int Id { get; }
            public string LocalizedTitle { get; }
            public IMvxCommand Command { get; }
        }

        public class MenuDescription : IEnumerable<MenuItemDescription>
        {
            private List<MenuItemDescription> MenuItems { get; } = new List<MenuItemDescription>();

            public MenuItemDescription this[int id] => this.MenuItems.SingleOrDefault(item => item.Id == id);
            public void Add(MenuItemDescription menuItem) => this.MenuItems.Add(menuItem);
            public void Add(int id, string localizedTitle) => this.Add(new MenuItemDescription(id, localizedTitle));

            public void Add(int id, string localizedTitle, IMvxCommand command)
                => this.Add(new MenuItemDescription(id, localizedTitle, command));

            public IEnumerator<MenuItemDescription> GetEnumerator() => this.MenuItems.GetEnumerator();
            IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable) this.MenuItems).GetEnumerator();
        }

        #endregion

        protected abstract int LanguagesMenuGroupId { get; }
        protected abstract int OriginalLanguageMenuItemId { get; }
        protected abstract int LanguagesMenuItemId { get; }
        protected abstract int MenuId { get; }
        protected abstract MenuDescription MenuDescriptor { get; }

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            this.Toolbar = this.FindViewById<Toolbar>(Resource.Id.toolbar);
            this.SetSupportActionBar(this.Toolbar);

            SetupAnswerTimeMeasurement();
        }

        public Toolbar Toolbar { get; private set; }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            this.answerAcceptedSubscription?.Dispose();
            this.answerAcceptedSubscription = null;
        }

        private void SetupAnswerTimeMeasurement()
        {
            if (this.answerAcceptedSubscription != null) return;

            this.answerAcceptedSubscription = messenger.SubscribeOnMainThread<AnswerAcceptedMessage>(ShowAnswerTime);
        }

        private Toast toast;
        private void ShowAnswerTime(AnswerAcceptedMessage msg)
        {
            if (!ViewModel.EnumeratorSettings.ShowAnswerTime) return;

            var message = string.Format(UIResources.AnswerRecordedMsg,
                NumericTextFormatter.FormatTimeHumanized(msg.Elapsed));

            toast?.Cancel();

            this.toast = Toast.MakeText(this, message, ToastLength.Short);
            toast.Show();
        }

        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            this.MenuInflater.Inflate(this.MenuId, menu);

            this.LocalizeMenuItems(menu);

            this.PopulateLanguagesMenu(menu);

            return base.OnCreateOptionsMenu(menu);
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
            => this.TryExecuteAssociatedCommand(item)
            || this.TrySwitchTranslation(item)
            || base.OnOptionsItemSelected(item);

        private void LocalizeMenuItems(IMenu menu)
        {
            foreach (var menuItem in this.MenuDescriptor)
            {
                menu.LocalizeMenuItem(menuItem.Id, menuItem.LocalizedTitle);
            }
        }

        protected void PopulateLanguagesMenu(IMenu menu)
        {
            if (!this.ViewModel.IsSuccessfullyLoaded)
                return;

            ISubMenu languagesMenu = menu.FindItem(this.LanguagesMenuItemId).SubMenu;

            IMenuItem currentLanguageMenuItem = menu.FindItem(this.OriginalLanguageMenuItemId);

            foreach (var language in this.ViewModel.AvailableLanguages)
            {
                var languageMenuItem = languagesMenu.Add(
                    groupId: this.LanguagesMenuGroupId,
                    itemId: Menu.None,
                    order: Menu.None,
                    title: language);

                if (language == this.ViewModel.CurrentLanguage)
                {
                    currentLanguageMenuItem = languageMenuItem;
                }
            }

            languagesMenu.SetGroupCheckable(this.LanguagesMenuGroupId, checkable: true, exclusive: true);

            currentLanguageMenuItem.SetChecked(true);
        }

        private bool TryExecuteAssociatedCommand(IMenuItem item)
        {
            var command = this.MenuDescriptor[item.ItemId]?.Command;

            if (command?.CanExecute() == true)
            {
                command.Execute();
                return true;
            }

            return false;
        }

        private bool TrySwitchTranslation(IMenuItem item)
        {
            if (item.GroupId == this.LanguagesMenuGroupId && !item.IsChecked)
            {
                var language =
                    item.ItemId == this.OriginalLanguageMenuItemId
                        ? null
                        : item.TitleFormatted.ToString();

                this.ViewModel.SwitchTranslationCommand.Execute(language);
                this.ViewModel.ReloadCommand.Execute();
                return true;
            }

            return false;
        }
    }
}
