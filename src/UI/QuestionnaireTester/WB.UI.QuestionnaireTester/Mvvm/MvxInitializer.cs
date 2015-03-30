using Cirrious.CrossCore;
using Microsoft.Practices.ServiceLocation;
using WB.Core.BoundedContexts.QuestionnaireTester.Services;
using WB.Core.BoundedContexts.QuestionnaireTester.Views;
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
            Mvx.RegisterSingleton(() => ServiceLocator.Current.GetInstance<IJsonUtils>());
            Mvx.RegisterSingleton(() => ServiceLocator.Current.GetInstance<IStringCompressor>());
            Mvx.RegisterSingleton(() => ServiceLocator.Current.GetInstance<IRestService>());

            Mvx.RegisterSingleton(() => ServiceLocator.Current.GetInstance<ILogger>());
            Mvx.RegisterSingleton(() => ServiceLocator.Current.GetInstance<IPrincipal>());
            Mvx.RegisterSingleton(() => ServiceLocator.Current.GetInstance<ICommandService>());
            Mvx.RegisterSingleton(() => ServiceLocator.Current.GetInstance<IQuestionnaireAssemblyFileAccessor>());
            Mvx.RegisterSingleton(() => ServiceLocator.Current.GetInstance<IAnswerProgressIndicator>());
            Mvx.RegisterSingleton(() => ServiceLocator.Current.GetInstance<IQueryablePlainStorageAccessor<QuestionnaireMetaInfoStorageViewModel>>());
            Mvx.RegisterSingleton(() => ServiceLocator.Current.GetInstance<IQueryablePlainStorageAccessor<QuestionnaireStorageViewModel>>());
            Mvx.RegisterSingleton(() => ServiceLocator.Current.GetInstance<IApplicationSettings>());
            Mvx.RegisterSingleton(() => ServiceLocator.Current.GetInstance<IRestServiceSettings>());
        }
    }
}