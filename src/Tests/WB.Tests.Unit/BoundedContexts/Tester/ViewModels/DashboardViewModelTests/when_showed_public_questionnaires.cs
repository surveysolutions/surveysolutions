using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using WB.Core.BoundedContexts.Tester.Implementation.Services;
using WB.Core.BoundedContexts.Tester.Services;
using WB.Core.BoundedContexts.Tester.ViewModels;
using WB.Core.BoundedContexts.Tester.Views;
using WB.Tests.Abc.Storage;

namespace WB.Tests.Unit.BoundedContexts.Tester.ViewModels.DashboardViewModelTests
{
    internal class when_showed_public_questionnaires : DashboardViewModelTestContext
    {
        [OneTimeSetUp]
        public async Task Establish()
        {
            var designerApiService = Mock.Of<IDesignerApiService>(
                _ => _.GetQuestionnairesAsync(Moq.It.IsAny<CancellationToken>()) == Task.FromResult(Questionnaires));

            var storageAccessor = new SqliteInmemoryStorage<QuestionnaireListItem>();

            viewModel = CreateDashboardViewModel(questionnaireListStorage: storageAccessor,
                designerApiService: designerApiService);
            await viewModel.Initialize();

            Because();
        }

        public void Because() => viewModel.ShowPublicQuestionnairesCommand.Execute();

        [Test] public void should_set_IsPublicShowed_to_true () => viewModel.IsPublicShowed.Should().BeTrue();
        [Test] public void should_Questionnaires_have_3_questionnaires () => viewModel.Questionnaires.Count.Should().Be(3);
        [Test] public void should_contains_only_public_questionnaires () => viewModel.Questionnaires.All(_ => _.IsPublic).Should().BeTrue();

        private static DashboardViewModel viewModel;

        private static readonly IReadOnlyCollection<QuestionnaireListItem> Questionnaires = new List<QuestionnaireListItem>
        {
            new QuestionnaireListItem { IsPublic = false, Id = "1"},
            new QuestionnaireListItem { IsPublic = false, Id = "2"},
            new QuestionnaireListItem {IsPublic = true, Id = "3"},
            new QuestionnaireListItem {IsPublic = true, Id = "4"},
            new QuestionnaireListItem {IsPublic = true, Id = "5"}
        };
    }
}
