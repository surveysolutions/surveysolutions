using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using WB.Core.BoundedContexts.Tester.Services;
using WB.Core.BoundedContexts.Tester.ViewModels;
using WB.Core.BoundedContexts.Tester.Views;
using WB.Tests.Abc.Storage;
using WB.Tests.Unit.BoundedContexts.Tester.ViewModels.DashboardViewModelTests;

namespace WB.Tests.Unit.BoundedContexts.Tester.ViewModels
{
    [TestOf(typeof(DashboardViewModel))]
    internal class DashboardViewModelTest : DashboardViewModelTestContext
    {
        [Test]
        public async Task when_showed_shared_with_me_questionnaires()
        {
            // arrange
            IReadOnlyCollection<QuestionnaireListItem> questionnaires = new List<QuestionnaireListItem>
            {
                new QuestionnaireListItem {IsPublic = true, Id = "1"},
                new QuestionnaireListItem {IsShared = true, Id = "2"},
                new QuestionnaireListItem {IsPublic = true, IsOwner = true, Id = "3"},
                new QuestionnaireListItem {IsShared = true, Id = "4"},
                new QuestionnaireListItem {IsOwner = true, Id = "5"}
            };

            var designerApiService = Mock.Of<IDesignerApiService>(
                _ => _.GetQuestionnairesAsync(Moq.It.IsAny<CancellationToken>()) == Task.FromResult(questionnaires));

            var storageAccessor = new SqliteInmemoryStorage<QuestionnaireListItem>();

            var viewModel = CreateDashboardViewModel(questionnaireListStorage: storageAccessor,
                designerApiService: designerApiService);
            await viewModel.Initialize();

            // act
            viewModel.ShowSharedWithMeCommand.Execute();

            // assert
            viewModel.Questionnaires.Count.Should().Be(2);
            viewModel.Questionnaires.All(_ => _.IsShared).Should().BeTrue();
        }

        [Test]
        public async Task when_showed_my_questionnaires_should_questionnaires_does_not_contains_shared_with_me()
        {
            // arrange
            IReadOnlyCollection<QuestionnaireListItem> questionnaires = new List<QuestionnaireListItem>
            {
                new QuestionnaireListItem {IsOwner = true, Id = "1"},
                new QuestionnaireListItem {IsShared = true, Id = "2"},
                new QuestionnaireListItem {IsPublic = true, IsOwner = true, Id = "3"},
                new QuestionnaireListItem {IsShared = true, Id = "4"},
                new QuestionnaireListItem {IsOwner = true, Id = "5"}
            };

            var designerApiService = Mock.Of<IDesignerApiService>(
                _ => _.GetQuestionnairesAsync(Moq.It.IsAny<CancellationToken>()) == Task.FromResult(questionnaires));

            var storageAccessor = new SqliteInmemoryStorage<QuestionnaireListItem>();

            var viewModel = CreateDashboardViewModel(questionnaireListStorage: storageAccessor,
                designerApiService: designerApiService);
            await viewModel.Initialize();

            // act
            viewModel.ShowMyQuestionnairesCommand.Execute();

            // assert
            viewModel.Questionnaires.Count.Should().Be(3);
            viewModel.Questionnaires.All(_ => _.IsShared).Should().BeFalse();
        }
    }
}
