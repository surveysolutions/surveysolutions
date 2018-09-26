using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Android.OS;
using Android.Views;
using Android.Widget;
using Humanizer;
using Humanizer.Localisation;
using MvvmCross;
using MvvmCross.Commands;
using MvvmCross.Plugin.Messenger;
using WB.Core.GenericSubdomains.Portable.ServiceLocation;
using WB.Core.SharedKernels.Enumerator.Properties;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails;
using Toolbar = Android.Support.V7.Widget.Toolbar;

namespace WB.UI.Shared.Enumerator.Activities
{
    public abstract class SingleInterviewActivity<TViewModel> : BaseActivity<TViewModel> where TViewModel : SingleInterviewViewModel
    {
        private IMvxMessenger Messenger => ServiceLocator.Current.GetInstance<IMvxMessenger>();
        private bool showAnswerAcceptedToast = true;
        private MvxSubscriptionToken answerAcceptedSubsribtion;

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

        protected Toolbar toolbar;

        protected abstract int LanguagesMenuGroupId { get; }
        protected abstract int OriginalLanguageMenuItemId { get; }
        protected abstract int LanguagesMenuItemId { get; }
        protected abstract int MenuId { get; }
        protected abstract MenuDescription MenuDescriptor { get; }

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            this.toolbar = this.FindViewById<Toolbar>(Resource.Id.toolbar);
            this.SetSupportActionBar(this.toolbar);

            SetupAnswerTimeMeasurement();
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            if (this.answerAcceptedSubsribtion != null)
            {
                Messenger.Unsubscribe<AnswerAcceptedMessage>(this.answerAcceptedSubsribtion);
                this.answerAcceptedSubsribtion.Dispose();
                this.answerAcceptedSubsribtion = null;

            }
        }

        private void SetupAnswerTimeMeasurement()
        {
            var settings = Mvx.Resolve<IEnumeratorSettings>();

            if (settings.ShowAnswerTime && answerAcceptedSubsribtion == null)
            {
                answerAcceptedSubsribtion = Messenger.Subscribe<AnswerAcceptedMessage>(msg =>
                {
                    if (showAnswerAcceptedToast)
                    {
                        var message = string.Format(UIResources.AnswerRecordedMsg,
                            msg.Elapsed.Humanize(maxUnit: TimeUnit.Minute));

                        this.RunOnUiThread(() => Toast.MakeText(this, message, ToastLength.Long).Show());
                        //var rootLayout = this.FindViewById(Resource.Id.rootLayout);
                        //Snackbar.Make(rootLayout,
                        //        message,
                        //        Snackbar.LengthIndefinite)
                        //    .SetAction(UIResources.AnswerRecordedMsgDismiss, view => { })
                        //    .Show();
                    }
                });
            }
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
