using System.IO;
using Cirrious.CrossCore.IoC;
using WB.Core.BoundedContexts.QuestionnaireTester;
using WB.Core.Infrastructure;
using WB.Core.Infrastructure.Files;

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