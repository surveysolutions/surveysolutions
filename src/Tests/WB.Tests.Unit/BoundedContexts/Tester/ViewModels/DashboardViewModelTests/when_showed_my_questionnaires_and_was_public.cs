using System;
using System.Collections.Generic;
using System.Linq;
using Machine.Specifications;
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
        public void Establish()
        {
            var storageAccessor = new SqliteInmemoryStorage<QuestionnaireListItem>();
            storageAccessor.Store(Questionnaires);

            viewModel = CreateDashboardViewModel(
                questionnaireListStorage: storageAccessor);

            viewModel.Load();
            viewModel.ShowPublicQuestionnairesCommand.Execute();

            Because();
        }

        public void Because() => viewModel.ShowMyQuestionnairesCommand.Execute();

        [Test] public void should_set_IsPublicShowed_to_false () => viewModel.IsPublicShowed.ShouldBeFalse();
        [Test] public void should_Questionnaires_have_2_questionnaires () => viewModel.Questionnaires.Count.ShouldEqual(2);
        [Test] public void should_contains_only_my_questionnaires () => viewModel.Questionnaires.All(_ => !_.IsPublic).ShouldBeTrue();

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