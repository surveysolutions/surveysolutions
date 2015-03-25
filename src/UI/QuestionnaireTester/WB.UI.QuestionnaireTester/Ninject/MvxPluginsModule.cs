using Chance.MvvmCross.Plugins.UserInteraction;
using Chance.MvvmCross.Plugins.UserInteraction.Droid;
using Cheesebaron.MvxPlugins.Settings.Droid;
using Cheesebaron.MvxPlugins.Settings.Interfaces;
using Cirrious.CrossCore.Droid.Platform;
using Cirrious.MvvmCross.Droid.Views;
using Cirrious.MvvmCross.Plugins.WebBrowser;
using Cirrious.MvvmCross.Plugins.WebBrowser.Droid;
using IHS.MvvmCross.Plugins.Keychain;
using IHS.MvvmCross.Plugins.Keychain.Droid;
using Ninject;
using Ninject.Modules;

namespace WB.UI.QuestionnaireTester.Ninject
{
    public class MvxPluginsModule : NinjectModule
    {
        public override void Load()
        {
            //this.Bind<IMvxAndroidCurrentTopActivity>().To<MvxAndroidLifetimeMonitor>().InSingletonScope();
            this.Bind<IMvxWebBrowserTask>().To<MvxWebBrowserTask>().InSingletonScope();
            this.Bind<IUserInteraction>().To<UserInteraction>().InSingletonScope();
            this.Bind<IKeychain>().To<DroidKeychain>().InSingletonScope();
            this.Bind<DroidCheeseSettingsConfiguration>().ToSelf().InSingletonScope();
            this.Bind<ISettings>().ToConstant(new Settings(this.Kernel.Get<DroidCheeseSettingsConfiguration>().SettingsFileName));
        }
    }
}