using Cirrious.CrossCore.IoC;
using WB.Core.BoundedContexts.QuestionnaireTester;
using WB.Core.Infrastructure;


using WB.Core.BoundedContexts.QuestionnaireTester;

namespace WB.UI.QuestionnaireTester.Ninject
{
    public class NinjectIoCAdapterSetup
    {
        public static IMvxIoCProvider CreateIocProvider()
        {
            return new NinjectMvxIocProvider(
                new SecurityModule(), 
                new NetworkModule(), 
                new LoggerModule(),
                new ApplicationModule(),
                new ServiceLocationModule(),
                new PlainStorageInfrastructureModule(),
                new MobileDataCollectionModule());
        }
    }
}