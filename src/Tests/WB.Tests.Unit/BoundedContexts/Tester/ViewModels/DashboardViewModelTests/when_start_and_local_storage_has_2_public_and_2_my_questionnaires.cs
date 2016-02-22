using System;
using System.Collections.Generic;
using System.Linq;
using Machine.Specifications;
using Nito.AsyncEx.Synchronous;
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
            var storageAccessor = new TestAsyncPlainStorage<QuestionnaireListItem>(MyQuestionnaires.Union(PublicQuestionnaires));

            viewModel = CreateDashboardViewModel(questionnaireListStorage: storageAccessor);
        };

        Because of = () => viewModel.StartAsync().WaitAndUnwrapException();

        It should_Questionnaires_have_my_questionnaires_only = () =>
            viewModel.Questionnaires.All(questionnaire => questionnaire.Id == firstMyQuestionnaire || questionnaire.Id == secondMyQuestionnaire).ShouldBeTrue();

        static DashboardViewModel viewModel;

        private static readonly string firstMyQuestionnaire = Guid.Parse("11111111111111111111111111111111").FormatGuid();
        private static readonly string secondMyQuestionnaire = Guid.Parse("22222222222222222222222222222222").FormatGuid();
        private static readonly IReadOnlyCollection<QuestionnaireListItem> MyQuestionnaires = new List<QuestionnaireListItem>
        {
            new QuestionnaireListItem(){Id = firstMyQuestionnaire, IsPublic = false, OwnerName = userName},
            new QuestionnaireListItem(){Id = secondMyQuestionnaire,  IsPublic = false, OwnerName = userName}
        };

        private static readonly IReadOnlyCollection<QuestionnaireListItem> PublicQuestionnaires = new List<QuestionnaireListItem>
        {
            new QuestionnaireListItem(){IsPublic = true},
            new QuestionnaireListItem(){IsPublic = true}
        };
    }
}