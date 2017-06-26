using System;
using System.Collections.Generic;
using System.Linq;
using Android.App;
using Android.Content.PM;
using Main.Core.Entities.SubEntities;
using Main.Core.Entities.SubEntities.Question;
using MvvmCross.Droid.Views;
using MvvmCross.Platform;
using WB.Core.BoundedContexts.Interviewer.Views;
using WB.Core.BoundedContexts.Interviewer.Views.Dashboard;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure.Storage;
using WB.Core.SharedKernels.Enumerator.Views;
using WB.Core.SharedKernels.Questionnaire.Translations;

namespace WB.UI.Interviewer.Activities
{
    [Activity(NoHistory = true, MainLauncher = true, ScreenOrientation = ScreenOrientation.Portrait, Theme = "@style/AppTheme")]
    public class SplashActivity : MvxSplashScreenActivity
    {
        public SplashActivity() : base(Resource.Layout.splash)
        {
        }

        protected override void TriggerFirstNavigate()
        {
            var logger = Mvx.Resolve<ILoggerProvider>().GetFor<SplashActivity>();
            logger.Warn($"Application started. Version: {typeof(SplashActivity).Assembly.GetName().Version}");

            this.BackwardCompatibility();
            Mvx.Resolve<IViewModelNavigationService>().NavigateToLogin();
        }

        private void BackwardCompatibility()
        {
            this.AddTitleToOptionViewForSearching();
        }

        private void AddTitleToOptionViewForSearching()
        {
            var optionsStorage = Mvx.Resolve<IPlainStorage<OptionView>>();

            var hasEmptySearchTitles = optionsStorage.Count(x => x.SearchTitle == null) > 0;
            if (!hasEmptySearchTitles) return;

            var allOptions = optionsStorage.LoadAll();

            foreach (var optionView in allOptions)
                optionView.SearchTitle = optionView.Title.ToLower();
            
            optionsStorage.Store(allOptions);

        }
    }
}