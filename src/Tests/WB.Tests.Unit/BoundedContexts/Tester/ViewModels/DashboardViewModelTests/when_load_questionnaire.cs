using System;
using System.Threading;
using System.Threading.Tasks;
using Main.Core.Documents;
using Moq;
using NUnit.Framework;
using WB.Core.BoundedContexts.Tester.Services;
using WB.Core.BoundedContexts.Tester.ViewModels;
using WB.Core.GenericSubdomains.Portable.Implementation;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.SharedKernels.DataCollection.Commands.Interview;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Questionnaire.Translations;
using WB.Core.SharedKernels.SurveySolutions.Api.Designer;
using WB.Core.SharedKernels.SurveySolutions.ReusableCategories;
using WB.Tests.Abc;

namespace WB.Tests.Unit.BoundedContexts.Tester.ViewModels.DashboardViewModelTests
{
    internal class when_load_questionnaire : DashboardViewModelTestContext
    {
        [OneTimeSetUp]
        public async Task Establish()
        {
            mockOfDesignerApiService.Setup(_ => _.GetQuestionnaireAsync(selectedQuestionnaire.Id, Moq.It.IsAny<IProgress<TransferProgress>>(),
                    Moq.It.IsAny<CancellationToken>())).Returns(Task.FromResult(downloadedQuestionnaire));

            var questionnaireRepository = new Mock<IQuestionnaireStorage>();
            questionnaireRepository.Setup(x => x.GetQuestionnaire(It.IsAny<QuestionnaireIdentity>(), null))
                .Returns(Create.Entity.PlainQuestionnaire(Create.Entity.QuestionnaireDocumentWithOneChapter()));

            viewModel = CreateDashboardViewModel(designerApiService: mockOfDesignerApiService.Object,
                commandService: mockOfCommandService.Object,
                questionnaireImportService: mockOfQuestionnaireImportService.Object,
                viewModelNavigationService: mockOfViewModelNavigationService.Object,
                questionnaireRepository: questionnaireRepository.Object
                );

            await Because();
        }

        public Task Because() => viewModel.LoadQuestionnaireCommand.ExecuteAsync(selectedQuestionnaire);

        [Test]
        public void should_be_downloaded_questionnaire() => 
            mockOfDesignerApiService.Verify(
                _ => _.GetQuestionnaireAsync(selectedQuestionnaire.Id, Moq.It.IsAny<IProgress<TransferProgress>>(), Moq.It.IsAny<CancellationToken>()), 
                Times.Once);

        [Test]
        public void should_be_questionnaire_stored_to_local_storage() => 
            mockOfQuestionnaireImportService.Verify(
                _ => _.ImportQuestionnaire(Moq.It.IsAny<QuestionnaireIdentity>(), downloadedQuestionnaire.Document, downloadedQuestionnaire.Assembly, new TranslationDto[0], new ReusableCategoriesDto[0]), 
                Times.Once);

        [Test]
        public void should_be_executed_CreateInterviewCommand() => 
            mockOfCommandService.Verify(_ => _.ExecuteAsync(Moq.It.IsAny<CreateInterview>(), null, Moq.It.IsAny<CancellationToken>()), Times.Once);

        
        private static DashboardViewModel viewModel;
        private static readonly Questionnaire downloadedQuestionnaire = new Questionnaire
        (
            document : new QuestionnaireDocument(),
            assembly :  "assembly"
        );
        private static readonly QuestionnaireListItemViewModel selectedQuestionnaire = new QuestionnaireListItemViewModel { Id = "11111111111111111111111111111111"};
        private static readonly Mock<IDesignerApiService> mockOfDesignerApiService = new Mock<IDesignerApiService>();
        private static readonly Mock<IQuestionnaireImportService> mockOfQuestionnaireImportService = new Mock<IQuestionnaireImportService>();
        private static readonly Mock<ICommandService> mockOfCommandService = new Mock<ICommandService>();
        private static readonly Mock<IViewModelNavigationService> mockOfViewModelNavigationService = new Mock<IViewModelNavigationService>();
    }
}
