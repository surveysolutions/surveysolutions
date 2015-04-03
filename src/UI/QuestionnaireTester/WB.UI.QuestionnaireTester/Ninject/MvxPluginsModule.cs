using Chance.MvvmCross.Plugins.UserInteraction;
using Chance.MvvmCross.Plugins.UserInteraction.Droid;
using Cheesebaron.MvxPlugins.Settings.Droid;
using Cheesebaron.MvxPlugins.Settings.Interfaces;
using Cirrious.MvvmCross.Plugins.Network.Droid;
using Cirrious.MvvmCross.Plugins.Network.Reachability;
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
            this.Bind<IUserInteraction>().To<UserInteraction>().InSingletonScope();
            this.Bind<IKeychain>().To<DroidKeychain>().InSingletonScope();
            this.Bind<DroidCheeseSettingsConfiguration>().ToSelf().InSingletonScope();
            this.Bind<ISettings>().ToConstant(new Settings(this.Kernel.Get<DroidCheeseSettingsConfiguration>().SettingsFileName));
            this.Bind<IMvxReachability>().To<MvxReachability>().InSingletonScope();
        }
    }
}