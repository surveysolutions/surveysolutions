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

namespace WB.Tests.Unit.BoundedContexts.Tester.ViewModels.DashboardViewModelTests
{
    internal class when_showed_public_questionnaires : DashboardViewModelTestContext
    {
        [Test]
        public async Task should_contain_only_public_questionnaires()
        {
            var designerApiService = Mock.Of<IDesignerApiService>(
                _ => _.GetQuestionnairesAsync(Moq.It.IsAny<CancellationToken>()) == Task.FromResult(Questionnaires));

            var storageAccessor = new SqliteInmemoryStorage<QuestionnaireListItem>();

            var viewModel = CreateDashboardViewModel(questionnaireListStorage: storageAccessor,
                designerApiService: designerApiService);
            await viewModel.Initialize();

            viewModel.ShowPublicQuestionnairesCommand.Execute();

            viewModel.Questionnaires.All(_ => _.IsPublic).Should().BeTrue();
            viewModel.Questionnaires.Count.Should().Be(3);
        }

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
