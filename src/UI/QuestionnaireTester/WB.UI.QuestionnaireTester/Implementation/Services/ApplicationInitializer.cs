using WB.Core.GenericSubdomains.Utils.Services;
using WB.UI.QuestionnaireTester.Mvvm;
using WB.UI.QuestionnaireTester.Ninject;

namespace WB.UI.QuestionnaireTester.Implementation.Services
{
    internal class ApplicationInitializer : IApplicationInitializer
    {
        public void Init()
        {
            NinjectInitializer.Initialize();
            MvxInitializer.Initialize();
        }
    }
}