using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Machine.Specifications;
using Moq;
using Nito.AsyncEx.Synchronous;
using WB.Core.BoundedContexts.Tester.Implementation.Services;
using WB.Core.BoundedContexts.Tester.ViewModels;
using WB.Core.BoundedContexts.Tester.Views;
using WB.Core.GenericSubdomains.Portable;
using WB.Tests.Unit.SharedKernels.SurveyManagement;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.BoundedContexts.Tester.ViewModels.DashboardViewModelTests
{
    internal class when_showed_public_questionnaires_and_local_storage_has_2_public_and_2_my_questionnaires : DashboardViewModelTestContext
    {
        Establish context = () =>
        {
            var designerApiService = Mock.Of<IDesignerApiService>(_ => 
                _.GetQuestionnairesAsync(false, Moq.It.IsAny<CancellationToken>()) == Task.FromResult(MyQuestionnaires) &&
                _.GetQuestionnairesAsync(true, Moq.It.IsAny<CancellationToken>()) == Task.FromResult(PublicQuestionnaires));

            var questionnairesStorage = new TestAsyncPlainStorage<QuestionnaireListItem>(MyQuestionnaires.Union(PublicQuestionnaires));

            viewModel = CreateDashboardViewModel(designerApiService: designerApiService,
                questionnaireListStorage: questionnairesStorage);
            viewModel.Start();
        };

        Because of = () => viewModel.ShowPublicQuestionnairesCommand.Execute();

        It should_Questionnaires_have_public_questionnaires_only = () =>
            viewModel.Questionnaires.All(questionnaire => questionnaire.Id == firstPublicQuestionnaire || questionnaire.Id == secondPublicQuestionnaire).ShouldBeTrue();

        static DashboardViewModel viewModel;

        private static readonly string firstPublicQuestionnaire = Guid.Parse("11111111111111111111111111111111").FormatGuid();
        private static readonly string secondPublicQuestionnaire = Guid.Parse("22222222222222222222222222222222").FormatGuid();
        private static readonly IList<QuestionnaireListItem> MyQuestionnaires = new List<QuestionnaireListItem>
        {
            new QuestionnaireListItem{IsPublic = false, OwnerName = userName},
            new QuestionnaireListItem{IsPublic = false, OwnerName = userName}
        };

        private static readonly IList<QuestionnaireListItem> PublicQuestionnaires = new List<QuestionnaireListItem>
        {
            new QuestionnaireListItem{Id = firstPublicQuestionnaire, IsPublic = true, OwnerName = userName},
            new QuestionnaireListItem{Id = secondPublicQuestionnaire, IsPublic = true, OwnerName = userName}
        };
    }
}