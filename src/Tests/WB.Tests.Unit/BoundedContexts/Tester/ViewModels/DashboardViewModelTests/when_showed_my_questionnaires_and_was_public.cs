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
    internal class when_showed_my_questionnaires_and_was_public : DashboardViewModelTestContext
    {
        Establish context = () =>
        {
            var storageAccessor = new SqliteInmemoryStorage<QuestionnaireListItem>();
            storageAccessor.StoreAsync(Questionnaires).WaitAndUnwrapException();

            viewModel = CreateDashboardViewModel(
                questionnaireListStorage: storageAccessor);

            viewModel.StartAsync().WaitAndUnwrapException();
            viewModel.ShowPublicQuestionnairesCommand.Execute();
        };

        Because of = () => viewModel.ShowMyQuestionnairesCommand.Execute();

        It should_set_IsPublicShowed_to_false = () => viewModel.IsPublicShowed.ShouldBeFalse();
        It should_Questionnaires_have_2_questionnaires = () => viewModel.Questionnaires.Count.ShouldEqual(2);
        It should_contains_only_my_questionnaires = () => viewModel.Questionnaires.All(_ => !_.IsPublic).ShouldBeTrue();

        private static DashboardViewModel viewModel;

        private static readonly IReadOnlyCollection<QuestionnaireListItem> Questionnaires = new List<QuestionnaireListItem>
        {
            new QuestionnaireListItem()
            {
                IsPublic = false,
                OwnerName = userName,
                Id = Guid.NewGuid().FormatGuid()
            },
            new QuestionnaireListItem()
            {
                IsPublic = false,
                OwnerName = userName,
                Id = Guid.NewGuid().FormatGuid()
            },
            new QuestionnaireListItem()
            {
                IsPublic = true,
                Id = Guid.NewGuid().FormatGuid()
            },
            new QuestionnaireListItem()
            {
                IsPublic = true,
                Id = Guid.NewGuid().FormatGuid()
            },
            new QuestionnaireListItem()
            {
                IsPublic = true,
                Id = Guid.NewGuid().FormatGuid()
            }
        };
    }
}