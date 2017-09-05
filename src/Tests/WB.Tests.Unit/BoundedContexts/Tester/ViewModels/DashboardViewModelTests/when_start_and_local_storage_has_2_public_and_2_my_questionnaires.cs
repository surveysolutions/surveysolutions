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
    internal class when_start_and_local_storage_has_2_public_and_2_my_questionnaires : DashboardViewModelTestContext
    {
        [OneTimeSetUp]
        public void Establish()
        {
            var storageAccessor = new SqliteInmemoryStorage<QuestionnaireListItem>();
            storageAccessor.Store(MyQuestionnaires.Union(PublicQuestionnaires));

            viewModel = CreateDashboardViewModel(questionnaireListStorage: storageAccessor);

            Because();
        }

        public void Because() => viewModel.Load();

        [Test]
        public void should_Questionnaires_have_my_questionnaires_only() =>
            viewModel.Questionnaires.All(questionnaire => questionnaire.Id == firstMyQuestionnaire ||
                                                          questionnaire.Id == secondMyQuestionnaire).ShouldBeTrue();

        static DashboardViewModel viewModel;

        static readonly string firstMyQuestionnaire = Guid.Parse("11111111111111111111111111111111").FormatGuid();
        static readonly string secondMyQuestionnaire = Guid.Parse("22222222222222222222222222222222").FormatGuid();

        static readonly IList<QuestionnaireListItem> MyQuestionnaires = new List<QuestionnaireListItem>
        {
            new QuestionnaireListItem() {Id = firstMyQuestionnaire, IsPublic = false, OwnerName = userName},
            new QuestionnaireListItem() {Id = secondMyQuestionnaire, IsPublic = false, OwnerName = userName}
        };

        static readonly IList<QuestionnaireListItem> PublicQuestionnaires = new List<QuestionnaireListItem>
        {
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