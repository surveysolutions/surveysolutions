using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Machine.Specifications;
using Moq;
using WB.Core.BoundedContexts.Tester.Implementation.Services;
using WB.Core.BoundedContexts.Tester.ViewModels;
using WB.Core.BoundedContexts.Tester.Views;
using WB.Core.GenericSubdomains.Portable;
using WB.Tests.Unit.SharedKernels.SurveyManagement;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.BoundedContexts.Tester.ViewModels.DashboardViewModelTests
{
    internal class when_start_and_local_storage_has_2_public_and_2_my_questionnaires : DashboardViewModelTestContext
    {
        Establish context = () =>
        {
            var designerApiService = Mock.Of<IDesignerApiService>(_ => 
                _.GetQuestionnairesAsync(false, Moq.It.IsAny<CancellationToken>()) == Task.FromResult(MyQuestionnaires) &&
                _.GetQuestionnairesAsync(true, Moq.It.IsAny<CancellationToken>()) == Task.FromResult(PublicQuestionnaires));

            var storageAccessor = new TestAsyncPlainStorage<QuestionnaireListItem>(MyQuestionnaires.Union(PublicQuestionnaires));

            viewModel = CreateDashboardViewModel(designerApiService: designerApiService,
                questionnaireListStorage: storageAccessor);
        };

        Because of = () => viewModel.Start();

        It should_Questionnaires_have_my_questionnaires_only = () =>
            viewModel.Questionnaires.All(questionnaire => questionnaire.Id == firstMyQuestionnaire || questionnaire.Id == secondMyQuestionnaire).ShouldBeTrue();

        static DashboardViewModel viewModel;

        private static readonly string firstMyQuestionnaire = Guid.Parse("11111111111111111111111111111111").FormatGuid();
        private static readonly string secondMyQuestionnaire = Guid.Parse("22222222222222222222222222222222").FormatGuid();
        private static readonly IList<QuestionnaireListItem> MyQuestionnaires = new List<QuestionnaireListItem>
        {
            new QuestionnaireListItem(){Id = firstMyQuestionnaire, IsPublic = false, OwnerName = userName},
            new QuestionnaireListItem(){Id = secondMyQuestionnaire,  IsPublic = false, OwnerName = userName}
        };

        private static readonly IList<QuestionnaireListItem> PublicQuestionnaires = new List<QuestionnaireListItem>
        {
            new QuestionnaireListItem(){IsPublic = true, OwnerName = userName},
            new QuestionnaireListItem(){IsPublic = true, OwnerName = userName}
        };
    }
}