using System;
using MvvmCross.Commands;
using MvvmCross.ViewModels;
using WB.Core.SharedKernels.Enumerator.Services;

namespace WB.Core.SharedKernels.Enumerator.ViewModels
{
    public class DashboardNotificationsViewModel : MvxNotifyPropertyChanged
    {
        public DashboardNotificationsViewModel(IViewModelNavigationService viewModelNavigationService,
            IEnumeratorSettings enumeratorSettings)
        {
            this.enumeratorSettings = enumeratorSettings;
            this.ViewModelNavigationService = viewModelNavigationService;
        }

        public IViewModelNavigationService ViewModelNavigationService { get; set; }

        private bool isNotificationPanelVisible;
        private readonly IEnumeratorSettings enumeratorSettings;

        public bool IsNotificationPanelVisible
        {
            get => this.isNotificationPanelVisible;
            set => SetProperty(ref this.isNotificationPanelVisible, value);
        }
        
        public IMvxCommand OpenSystemSettingsDateAdjust => 
            new MvxCommand(() => ViewModelNavigationService.NavigateToSystemDateSettings());
        public void CheckTabletTimeAndWarn()
        {
            var allowedThresholdInSeconds = 180 * 24 * 60 * 60;
            
            long? lastHqSyncTimestamp = enumeratorSettings.LastHqSyncTimestamp;
            var nowSeconds = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

            IsNotificationPanelVisible = 
                lastHqSyncTimestamp != null 
                && (nowSeconds > lastHqSyncTimestamp + allowedThresholdInSeconds
                    || nowSeconds < lastHqSyncTimestamp - allowedThresholdInSeconds);
        }
    }
}
