using Main.Core.Documents;
using Microsoft.Practices.ServiceLocation;
using WB.Core.BoundedContexts.QuestionnaireTester.ViewModels;
using WB.Core.GenericSubdomains.Utils.Services;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Implementation.Accessors;

namespace WB.UI.QuestionnaireTester.Mvvm
{
    public class MvxInitializer
    {
        public static void Initialize()
        {
            Cirrious.CrossCore.Mvx.RegisterSingleton(() => ServiceLocator.Current.GetInstance<IJsonUtils>());
            Cirrious.CrossCore.Mvx.RegisterSingleton(() => ServiceLocator.Current.GetInstance<IStringCompressor>());
            Cirrious.CrossCore.Mvx.RegisterSingleton(() => ServiceLocator.Current.GetInstance<IRestService>());

            Cirrious.CrossCore.Mvx.RegisterSingleton(() => ServiceLocator.Current.GetInstance<ILogger>());
            Cirrious.CrossCore.Mvx.RegisterSingleton(() => ServiceLocator.Current.GetInstance<IPrincipal>());
            Cirrious.CrossCore.Mvx.RegisterSingleton(() => ServiceLocator.Current.GetInstance<ICommandService>());
            Cirrious.CrossCore.Mvx.RegisterSingleton(() => ServiceLocator.Current.GetInstance<IQuestionnaireAssemblyFileAccessor>());
            Cirrious.CrossCore.Mvx.RegisterSingleton(() => ServiceLocator.Current.GetInstance<IAnswerProgressIndicator>());
            Cirrious.CrossCore.Mvx.RegisterSingleton(() => ServiceLocator.Current.GetInstance<IPlainStorageAccessor<DashboardStorageViewModel>>());
            Cirrious.CrossCore.Mvx.RegisterSingleton(() => ServiceLocator.Current.GetInstance<IPlainStorageAccessor<QuestionnaireDocument>>());
        }
    }
}