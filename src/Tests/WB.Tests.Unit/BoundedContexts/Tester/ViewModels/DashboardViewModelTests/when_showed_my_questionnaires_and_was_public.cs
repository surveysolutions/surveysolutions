using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using NUnit.Framework;
using WB.Core.BoundedContexts.Tester.ViewModels;
using WB.Core.BoundedContexts.Tester.Views;
using WB.Core.GenericSubdomains.Portable;
using WB.Tests.Abc.Storage;

namespace WB.Tests.Unit.BoundedContexts.Tester.ViewModels.DashboardViewModelTests
{
    internal class when_showed_my_questionnaires_and_was_public : DashboardViewModelTestContext
    {
        [OneTimeSetUp]
        public async Task Establish()
        {
            var storageAccessor = new SqliteInmemoryStorage<QuestionnaireListItem>();
            storageAccessor.Store(Questionnaires);

            viewModel = CreateDashboardViewModel(
                questionnaireListStorage: storageAccessor);

            await viewModel.Initialize();
            viewModel.ShowPublicQuestionnairesCommand.Execute();

            Because();
        }

        public void Because() => viewModel.ShowMyQuestionnairesCommand.Execute();

        [Test] public void should_Questionnaires_have_2_questionnaires () => viewModel.Questionnaires.Count.Should().Be(2);
        [Test] public void should_contains_only_my_questionnaires () => viewModel.Questionnaires.All(_ => !_.IsPublic).Should().BeTrue();

        private static DashboardViewModel viewModel;

        private static readonly IReadOnlyCollection<QuestionnaireListItem> Questionnaires = new List<QuestionnaireListItem>
        {
            new QuestionnaireListItem()
            {
                IsPublic = false,
                OwnerName = userName,
                IsOwner = true,
                Id = Guid.NewGuid().FormatGuid()
            },
            new QuestionnaireListItem()
            {
                IsPublic = false,
                OwnerName = userName,
                IsOwner = true,
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
