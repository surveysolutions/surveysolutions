using System;
using Ninject.Modules;
using WB.Core.GenericSubdomains.Utils.Services;
using WB.UI.QuestionnaireTester.Implementation.Services;

namespace WB.UI.QuestionnaireTester.Ninject
{
    public class LoggerModule : NinjectModule
    {
        public override void Load()
        {
            this.Bind<ILogger>().To<XamarinInsightsLogger>().InSingletonScope();
        }
    }
}