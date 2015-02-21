using System;
using Chance.MvvmCross.Plugins.UserInteraction;
using Cirrious.MvvmCross.ViewModels;
using WB.Core.BoundedContexts.QuestionnaireTester.Properties;
using WB.Core.GenericSubdomains.Utils.Services;

namespace WB.Core.BoundedContexts.QuestionnaireTester.ViewModels
{
    public class SettingsViewModel : BaseViewModel
    {
        private readonly IRestServiceSettings restServiceSettings;

        public SettingsViewModel(ILogger logger, IPrincipal principal, IRestServiceSettings restServiceSettings,
            IUserInteraction uiDialogs) : base(logger, principal: principal, uiDialogs: uiDialogs)
        {
            this.restServiceSettings = restServiceSettings;
            this.LoadSettingsFromStorage();
        }

        private string endpoint;
        public string Endpoint
        {
            get { return endpoint; }
            set { endpoint = value; RaisePropertyChanged(() => Endpoint); }
        }

        private int timeout;
        public int Timeout
        {
            get { return timeout; }
            set { timeout = value; RaisePropertyChanged(() => Timeout); }
        }

        private int bufferSize;
        public int BufferSize
        {
            get { return bufferSize; }
            set { bufferSize = value; RaisePropertyChanged(() => BufferSize); }
        }

        private bool allowToSendInfoByCrashes = true;
        public bool AllowToSendInfoByCrashes
        {
            get { return allowToSendInfoByCrashes; }
            set { allowToSendInfoByCrashes = value; RaisePropertyChanged(() => BufferSize); }
        }


        private IMvxCommand saveSettingsCommand;
        public IMvxCommand SaveSettingsCommand
        {
            get { return saveSettingsCommand ?? (saveSettingsCommand = new MvxCommand(this.SaveSettings)); }
        }

        private IMvxCommand resetSettingsCommand;
        public IMvxCommand ResetSettingsCommand
        {
            get { return resetSettingsCommand ?? (resetSettingsCommand = new MvxCommand(this.ResetSettings)); }
        }

        private void LoadSettingsFromStorage()
        {
            this.Endpoint = this.restServiceSettings.Endpoint;
            this.Timeout = this.restServiceSettings.Timeout.Seconds;
            this.BufferSize = this.restServiceSettings.BufferSize;
        }

        private void ResetSettings()
        {
            this.UIDialogs.Confirm(UIResources.ResetSettingsConfirmationText, () =>
            {
                this.restServiceSettings.Endpoint = this.Endpoint;
                this.restServiceSettings.Timeout = new TimeSpan(0, 0, 30);
                this.restServiceSettings.BufferSize = 512;

                this.LoadSettingsFromStorage();

            }, UIResources.ConfirmationText, UIResources.ConfirmationYesText, UIResources.ConfirmationNoText);
        }

        private void SaveSettings()
        {
            this.restServiceSettings.Endpoint = this.Endpoint;
            this.restServiceSettings.Timeout = new TimeSpan(0, 0, this.Timeout);
            this.restServiceSettings.BufferSize = this.BufferSize;

            this.UIDialogs.Alert(UIResources.SaveSettingsSuccessText);
        }
    }
}