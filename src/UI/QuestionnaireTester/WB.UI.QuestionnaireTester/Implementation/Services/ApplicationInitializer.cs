using Cirrious.CrossCore;
using Cirrious.CrossCore.Droid.Platform;
using WB.Core.GenericSubdomains.Utils.Services;
using WB.UI.QuestionnaireTester.Mvvm;
using WB.UI.QuestionnaireTester.Ninject;
using Xamarin;

namespace WB.UI.QuestionnaireTester.Implementation.Services
{
    internal class ApplicationInitializer : IApplicationInitializer
    {
        public void Init()
        {
            Insights.Initialize("24d22f99f3068798f24f20d297baaa0fbfe9f528", Mvx.Resolve<IMvxAndroidCurrentTopActivity>().Activity);
            NinjectInitializer.Initialize();
            MvxInitializer.Initialize();
        }
    }
}