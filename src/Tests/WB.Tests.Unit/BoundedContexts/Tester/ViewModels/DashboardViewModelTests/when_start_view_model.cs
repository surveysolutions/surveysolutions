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
using WB.Tests.Abc.Storage;

namespace WB.Tests.Unit.BoundedContexts.Tester.ViewModels.DashboardViewModelTests
{
    internal class when_start_view_model : DashboardViewModelTestContext
    {
        [OneTimeSetUp]
        public async Task Establish()
        {
            var designerApiService = Mock.Of<IDesignerApiService>(
                _ => _.GetQuestionnairesAsync(Moq.It.IsAny<CancellationToken>()) == Task.FromResult(Questionnaires));

            var storageAccessor = new SqliteInmemoryStorage<QuestionnaireListItem>();

            storageAccessor.Store(new List<QuestionnaireListItem>
            {
                new QuestionnaireListItem
                {
                    IsPublic = false,
                    OwnerName = userName,
                    IsOwner = true,
                    Id = Guid.NewGuid().FormatGuid()
                },
                new QuestionnaireListItem
                {
                    IsPublic = false,
                    OwnerName = userName,
                    IsOwner = true,
                    Id = Guid.NewGuid().FormatGuid()
                },
                new QuestionnaireListItem
                {
                    IsPublic = true,
                    Id = Guid.NewGuid().FormatGuid()
                },
                new QuestionnaireListItem
                {
                    IsPublic = true,
                    Id = Guid.NewGuid().FormatGuid()
                },
                new QuestionnaireListItem
                {
                    IsPublic = true,
                    Id = Guid.NewGuid().FormatGuid()
                }
            });

            viewModel = CreateDashboardViewModel(designerApiService: designerApiService,
                questionnaireListStorage: storageAccessor);

            await Because();
        }

        public async Task Because() => await viewModel.Initialize();

        [Test] public void should_set_ShowEmptyQuestionnaireListText_to_true() => viewModel.ShowEmptyQuestionnaireListText.Should().BeTrue();
        [Test] public void should_set_IsPublicShowed_to_false() => viewModel.IsPublicShowed.Should().BeFalse();
        [Test] public void should_Questionnaires_have_2_questionnaires() => viewModel.Questionnaires.Count.Should().Be(2);
        [Test] public void should_contains_only_my_questionnares() => viewModel.Questionnaires.All(_ => !_.IsPublic).Should().BeTrue();
        [Test] public void should_set_MyQuestionnairesCount_to_2() => viewModel.MyQuestionnairesCount.Should().Be(2);
        [Test] public void should_set_PublicQuestionnairesCount_to_3() => viewModel.PublicQuestionnairesCount.Should().Be(3);

        private static DashboardViewModel viewModel;

        private static readonly IReadOnlyCollection<QuestionnaireListItem> Questionnaires = new List<QuestionnaireListItem>
        {
            new QuestionnaireListItem(){IsPublic = false, IsOwner = true, OwnerName = userName},
            new QuestionnaireListItem(){IsPublic = false, IsOwner = true, OwnerName = userName},
            new QuestionnaireListItem(){IsPublic = true},
            new QuestionnaireListItem(){IsPublic = true},
            new QuestionnaireListItem(){IsPublic = true}
        };
    }
}
