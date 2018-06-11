using System;
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
using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Portable.Tasks;
using WB.Tests.Abc.Storage;

namespace WB.Tests.Unit.BoundedContexts.Tester.ViewModels.DashboardViewModelTests
{
    internal class when_refresh_questionnaire_list : DashboardViewModelTestContext
    {
        [OneTimeSetUp]
        public void Establish()
        {
            var designerApiService = Mock.Of<IDesignerApiService>(
                _ => _.GetQuestionnairesAsync(Moq.It.IsAny<CancellationToken>()) == Task.FromResult(Questionnaires));

            var storageAccessor = new SqliteInmemoryStorage<QuestionnaireListItem>();

            viewModel = CreateDashboardViewModel(questionnaireListStorage: storageAccessor,
                designerApiService: designerApiService);

            viewModel.ShowPublicQuestionnairesCommand.Execute();
            Because();
        }

        public void Because() => viewModel.RefreshQuestionnairesCommand.ExecuteAsync().WaitAndUnwrapException();

        [Test] public void should_stay_on_same_tab_with_public_questionnaires () => viewModel.IsPublicShowed.Should().BeTrue();
        [Test] public void should_Questionnaires_have_3_public_questionnaires () => viewModel.Questionnaires.Count.Should().Be(3);
        [Test] public void should_contains_only_public_questionnaires () => viewModel.Questionnaires.All(_ => _.IsPublic).Should().BeTrue();
        [Test] public void should_set_MyQuestionnairesCount_to_2 () => viewModel.MyQuestionnairesCount.Should().Be(2);
        [Test] public void should_set_PublicQuestionnairesCount_to_3 () => viewModel.PublicQuestionnairesCount.Should().Be(3);

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
