using Cirrious.CrossCore.IoC;

namespace WB.UI.QuestionnaireTester.Ninject
{
    public class NinjectIoCAdapterSetup
    {
        public static IMvxIoCProvider CreateIocProvider()
        {
             return new NinjectMvxIocProvider(new SecurityModule(), new MvxPluginsModule(), new NetworkModule(), new LoggerModule(), new CommonModule());
        }
    }
}