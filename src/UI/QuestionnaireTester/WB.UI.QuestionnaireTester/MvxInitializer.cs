using Cirrious.CrossCore;
using Main.Core.Documents;
using Microsoft.Practices.ServiceLocation;
using WB.Core.GenericSubdomains.Utils.Services;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.DataCollection.Implementation.Accessors;
using WB.UI.QuestionnaireTester.ViewModels;
using WB.UI.Shared.Android.Controls.ScreenItems;

namespace WB.UI.QuestionnaireTester
{
    public class MvxInitializer
    {
        public static void Initialize()
        {
            Mvx.RegisterSingleton(() => ServiceLocator.Current.GetInstance<IJsonUtils>());
            Mvx.RegisterSingleton(() => ServiceLocator.Current.GetInstance<IStringCompressor>());
            Mvx.RegisterSingleton(() => ServiceLocator.Current.GetInstance<IRestService>());

            Mvx.RegisterSingleton(() => ServiceLocator.Current.GetInstance<ILogger>());
            Mvx.RegisterSingleton(() => ServiceLocator.Current.GetInstance<IPrincipal>());
            Mvx.RegisterSingleton(() => ServiceLocator.Current.GetInstance<ICommandService>());
            Mvx.RegisterSingleton(() => ServiceLocator.Current.GetInstance<IQuestionnaireAssemblyFileAccessor>());
            Mvx.RegisterSingleton(() => ServiceLocator.Current.GetInstance<IAnswerProgressIndicator>());
            Mvx.RegisterSingleton(() => ServiceLocator.Current.GetInstance<IPlainStorageAccessor<DashboardStorageViewModel>>());
            Mvx.RegisterSingleton(() => ServiceLocator.Current.GetInstance<IPlainStorageAccessor<QuestionnaireDocument>>());
        }
    }
}