using System;
using FluentAssertions;
using Main.Core.Documents;
using Moq;
using WB.Core.BoundedContexts.Designer.MembershipProvider;
using WB.Core.BoundedContexts.Designer.Services;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit.QuestionnaireInfo;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.QuestionnaireList;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.SharedPersons;
using WB.Core.Infrastructure.Implementation;
using WB.Core.Infrastructure.PlainStorage;


namespace WB.Tests.Unit.Designer.BoundedContexts.Designer.QuestionnaireInfoViewFactoryTests
{
    internal class when_loading_view_and_shared_persons_contains_viewer : QuestionnaireInfoViewFactoryContext
    {
        [NUnit.Framework.Test]
        public void should_be_only_1_specified_shared_person()
        {
            var questionnaireInfoViewRepository = Mock.Of<IDesignerQuestionnaireStorage>(
                x => x.Get(questionnaireId) == CreateQuestionnaireDocument(questionnaireId.ToString(), questionnaireTitle));

            var dbContext = Create.InMemoryDbContext();
            var questionnaireListViewItem = Create.QuestionnaireListViewItem(id:questionnaireId.QuestionnaireId);
            questionnaireListViewItem.SharedPersons.Add(new SharedPerson
            {
                UserId = userId,
                Email = userEmail,
                IsOwner = false
            });
            dbContext.Questionnaires.Add(questionnaireListViewItem);

            dbContext.Users.Add(new DesignerIdentityUser { Id = userId, Email = userEmail });
            dbContext.SaveChanges();

            factory = CreateQuestionnaireInfoViewFactory(repository: questionnaireInfoViewRepository,
                dbContext);

            BecauseOf();

            view.SharedPersons.Count.Should().Be(1);
            view.SharedPersons[0].Email.Should().Be(userEmail);
        }

        private void BecauseOf() => view = factory.Load(questionnaireId, userId);

        private static QuestionnaireInfoView view;
        private static QuestionnaireInfoViewFactory factory;
        private static string questionnaireTitle = "questionnaire title";
        private static readonly Guid userId = Guid.Parse("22222222222222222222222222222222");
        private static string userEmail = "user@email.com";
    }
}
