using System;
using MvvmCross.Commands;
using MvvmCross.ViewModels;
using Ncqrs;
using WB.Core.SharedKernels.Enumerator.Services;

namespace WB.Core.SharedKernels.Enumerator.ViewModels
{
    public class DashboardNotificationsViewModel : MvxNotifyPropertyChanged
    {
        public DashboardNotificationsViewModel(IViewModelNavigationService viewModelNavigationService,
            IEnumeratorSettings enumeratorSettings,
            IClock clock)
        {
            this.enumeratorSettings = enumeratorSettings;
            this.ViewModelNavigationService = viewModelNavigationService;
            this.Clock = clock;
        }

        private IClock Clock { get; set; }

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
            var allowedThresholdInSeconds = TimeSpan.FromMinutes(80).TotalSeconds;
            
            long? lastHqSyncTimestamp = enumeratorSettings.LastHqSyncTimestamp;
            var nowSeconds =  Clock.DateTimeOffsetNow().ToUnixTimeSeconds();

            IsNotificationPanelVisible = lastHqSyncTimestamp != null 
                                         && nowSeconds <= lastHqSyncTimestamp - allowedThresholdInSeconds;
        }
    }
}
