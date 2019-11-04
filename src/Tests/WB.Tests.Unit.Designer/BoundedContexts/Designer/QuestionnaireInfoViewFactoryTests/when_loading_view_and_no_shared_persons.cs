using System;
using FluentAssertions;
using Main.Core.Documents;
using Moq;
using WB.Core.BoundedContexts.Designer.MembershipProvider;
using WB.Core.BoundedContexts.Designer.Services;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.ChangeHistory;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit.QuestionnaireInfo;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.PlainStorage;
using WB.Tests.Abc;


namespace WB.Tests.Unit.Designer.BoundedContexts.Designer.QuestionnaireInfoViewFactoryTests
{
    internal class when_loading_view_and_no_shared_persons : QuestionnaireInfoViewFactoryContext
    {
        [NUnit.Framework.Test] public void should_be_only_owner_in_shared_persons_list() {
            var questionnaireDocument = CreateQuestionnaireDocument(questionnaireId.QuestionnaireId.FormatGuid(), questionnaireTitle);
            questionnaireDocument.CreatedBy = userId;

            var questionnaireInfoViewRepository = Mock.Of<IDesignerQuestionnaireStorage>(
                x => x.Get(questionnaireId) == questionnaireDocument);

            var dbContext = Create.InMemoryDbContext();
            dbContext.Users.Add(new DesignerIdentityUser { Id = userId, Email = userEmail });
            dbContext.Questionnaires.Add(Create.QuestionnaireListViewItem(id: questionnaireId.QuestionnaireId, createdBy: userId));

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
