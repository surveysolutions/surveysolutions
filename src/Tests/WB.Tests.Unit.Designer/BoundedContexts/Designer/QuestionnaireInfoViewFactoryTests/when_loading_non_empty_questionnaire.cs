using System;
using FluentAssertions;
using Main.Core.Documents;
using Main.Core.Entities.SubEntities;
using Main.Core.Entities.SubEntities.Question;
using Moq;
using NUnit.Framework;
using WB.Core.BoundedContexts.Designer.Aggregates;
using WB.Core.BoundedContexts.Designer.MembershipProvider;
using WB.Core.BoundedContexts.Designer.Services;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.ChangeHistory;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit.QuestionnaireInfo;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.PlainStorage;
using WB.Tests.Abc;

namespace WB.Tests.Unit.Designer.BoundedContexts.Designer.QuestionnaireInfoViewFactoryTests
{
    internal class when_loading_non_empty_questionnaire : QuestionnaireInfoViewFactoryContext
    {
        [OneTimeSetUp]
        public void context()
        {
            var questionnaire = Create.QuestionnaireDocumentWithOneChapter(
                new NumericQuestion(),
                new Group("Roster")
                {
                    IsRoster = true
                });
            questionnaire.Title = questionnaireTitle;
            questionnaire.CreatedBy = userId;
            questionnaire.DefaultTranslation = Id.gA;
            questionnaire.Translations.Add(new Core.SharedKernels.SurveySolutions.Documents.Translation
            {
                Id = Id.gA,
                Name = "Default"
            });
            questionnaire.Translations.Add(new Core.SharedKernels.SurveySolutions.Documents.Translation
            {
                Id = Id.gB,
                Name = "NotDefault"
            });
            var repositoryMock = new Mock<IDesignerQuestionnaireStorage>();
            repositoryMock
                .Setup(x => x.Get(questionnaireId))
                .Returns(questionnaire);

            var dbContext = Create.InMemoryDbContext();
            dbContext.Users.Add(new DesignerIdentityUser() {Id = userId, Email = ownerEmail});
            dbContext.Questionnaires.Add(Create.QuestionnaireListViewItem(id: questionnaireId.QuestionnaireId, createdBy: userId));
            dbContext.SaveChanges();

            factory = CreateQuestionnaireInfoViewFactory(repository: repositoryMock.Object,
                dbContext);
            BecauseOf();
        }

        private void BecauseOf() => view = factory.Load(questionnaireId, userId);

        [Test]
        public void should_count_number_of_questions_in_questionnaire
            () => view.QuestionsCount.Should().Be(1);

        [Test]
        public void should_count_number_of_groups_in_questionnaire
            () => view.GroupsCount.Should().Be(1);

        [Test]
        public void should_count_number_of_roster_in_questionnaire
            () => view.RostersCount.Should().Be(1);

        [Test]
        public void should_contain_email_of_first_element_in_list_of_shared_persons_equal_to_owner_email
            () => view.SharedPersons[0].Email.Should().Be(ownerEmail);

        [Test]
        public void should_contain_id_of_first_element_in_list_of_shared_persons_equal_to_owner_id
            () => view.SharedPersons[0].UserId.Should().Be(userId);

        [Test]
        public void should_contain_isOwner_of_first_element_in_list_of_shared_persons_be_true
            () => view.SharedPersons[0].IsOwner.Should().Be(true);

        [Test]
        public void should_contain_default_translation_value
            () => view.Translations[0].IsDefault.Should().Be(true);

        [Test]
        public void should_not_contain_default_translation_value_for_non_default_translations
            () => view.Translations[1].IsDefault.Should().Be(false);

        private static QuestionnaireInfoView view;
        private static QuestionnaireInfoViewFactory factory;
        
        private static string questionnaireTitle = "questionnaire title";
        private static Guid userId = Guid.Parse("22222222222222222222222222222222");
        private static string ownerEmail = "r@example.org";
    }
}
