using Cirrious.CrossCore;
using Cirrious.CrossCore.Plugins;
using Cirrious.MvvmCross.Plugins.Location;

namespace WB.UI.QuestionnaireTester.CustomServices.Location
{
    public class Plugin: IMvxPlugin
    {
        public void Load()
        {
            Mvx.RegisterSingleton<IMvxLocationWatcher>(() => new PlayServicesLocationWatcher());
        }
    }
}