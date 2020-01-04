using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using WB.Core.BoundedContexts.Tester.Services;
using WB.Core.BoundedContexts.Tester.Views;
using WB.Tests.Abc.Storage;
using WB.Core.BoundedContexts.Tester.ViewModels;
namespace WB.Tests.Unit.BoundedContexts.Tester.ViewModels.DashboardViewModelTests
{
    internal class when_showed_public_questionnaires : DashboardViewModelTestContext
    {
        [Test]
        public async Task should_contain_only_public_questionnaires()
        {
            var designerApiService = new Mock<IDesignerApiService>();
            var tcs = new TaskCompletionSource<bool>();

            designerApiService.Setup(x => x.GetQuestionnairesAsync(It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(Questionnaires));

            var storageAccessor = new SqliteInmemoryStorage<QuestionnaireListItem>();

            var viewModel = CreateDashboardViewModel(
                questionnaireListStorage: storageAccessor,
                designerApiService: designerApiService.Object);

            viewModel.PropertyChanged += (sender, args) =>
            {
                if (args.PropertyName == nameof(DashboardViewModel.Questionnaires))
                {
                    tcs.TrySetResult(true);
                }
            };

            await viewModel.Initialize();
            await tcs.Task; // flakiness fix

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
