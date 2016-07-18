using System;
using System.Threading;
using System.Threading.Tasks;

using Machine.Specifications;

using Moq;
using WB.Core.BoundedContexts.Tester.Implementation.Services;
using WB.Core.BoundedContexts.Tester.Services;
using WB.Core.BoundedContexts.Tester.ViewModels;
using WB.Core.GenericSubdomains.Portable.Implementation;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure.Storage;
using WB.Core.SharedKernels.SurveySolutions.Api.Designer;
using It = Machine.Specifications.It;
using QuestionnaireListItem = WB.Core.BoundedContexts.Tester.Views.QuestionnaireListItem;
using TranslationInstance = WB.Core.SharedKernels.Enumerator.Views.TranslationInstance;

namespace WB.Tests.Unit.BoundedContexts.Tester.ViewModels.DashboardViewModelTests
{
    internal class when_load_questionnaire_with_translations : DashboardViewModelTestContext
    {
        Establish context = () =>
        {
            downloadedQuestionnaire = new Questionnaire
            {
                Document = Create.Entity.QuestionnaireDocumentWithTranslations(translations: new[]
                {
                    Create.Entity.Translation(translationId: Guid.Parse(translationId),  translationName: "ru")
                }),
                Assembly = "assembly"
            };

            mockOfDesignerApiService
                .Setup(_ => _.GetQuestionnaireAsync(selectedQuestionnaire, Moq.It.IsAny<Action<DownloadProgressChangedEventArgs>>(), Moq.It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(downloadedQuestionnaire));
            mockOfDesignerApiService
                .Setup(_ => _.GetTranslationsAsync(selectedQuestionnaire.Id, Moq.It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(new[]
                {
                    Create.Entity.TranslationDto(language: translationId, value: "перевод на русский"),
                    Create.Entity.TranslationDto(language: translationId, value: "перевод на русский 2")

                }));

            viewModel = CreateDashboardViewModel(designerApiService: mockOfDesignerApiService.Object,
                commandService: mockOfCommandService.Object,
                questionnaireImportService: mockOfQuestionnaireImportService.Object,
                viewModelNavigationService: mockOfViewModelNavigationService.Object,
                translationsStorage: mockOfTranslationsStorage.Object
                );
        };

        Because of = () => viewModel.LoadQuestionnaireCommand.Execute(selectedQuestionnaire);

        It should_remove_all_translations_from_local_storage = () =>
            mockOfTranslationsStorage.Verify(_ => _.RemoveAllAsync(), Times.Once);

        It should_store_2_tranlations_by_russian_language_to_local_storage = () =>
            mockOfTranslationsStorage.Verify(_ => _.StoreAsync(Moq.It.Is<TranslationInstance>(x=>x.Language == translationId)), Times.Exactly(2));
        
        private static DashboardViewModel viewModel;
        private static Questionnaire downloadedQuestionnaire;

        private static readonly QuestionnaireListItem selectedQuestionnaire = new QuestionnaireListItem() { Id = "11111111111111111111111111111111"};
        private static readonly string translationId = Guid.Parse("22222222222222222222222222222222").ToString();
        private static readonly Mock<IDesignerApiService> mockOfDesignerApiService = new Mock<IDesignerApiService>();
        private static readonly Mock<IQuestionnaireImportService> mockOfQuestionnaireImportService = new Mock<IQuestionnaireImportService>();
        private static readonly Mock<ICommandService> mockOfCommandService = new Mock<ICommandService>();
        private static readonly Mock<IViewModelNavigationService> mockOfViewModelNavigationService = new Mock<IViewModelNavigationService>();
        private static readonly Mock<IAsyncPlainStorage<TranslationInstance>> mockOfTranslationsStorage = new Mock<IAsyncPlainStorage<TranslationInstance>>();
    }
}