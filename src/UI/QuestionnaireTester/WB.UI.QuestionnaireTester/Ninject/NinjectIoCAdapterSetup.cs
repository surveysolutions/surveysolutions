using Cirrious.CrossCore.IoC;
using WB.Core.BoundedContexts.QuestionnaireTester;

namespace WB.UI.QuestionnaireTester.Ninject
{
    public class NinjectIoCAdapterSetup
    {
        public static IMvxIoCProvider CreateIocProvider()
        {
             return new NinjectMvxIocProvider(new ServiceLocationModule(),
                new MvxPluginsModule(),
                new TesterLoggingModule(),
                new AndroidTesterModelModule());
        }
    }
}