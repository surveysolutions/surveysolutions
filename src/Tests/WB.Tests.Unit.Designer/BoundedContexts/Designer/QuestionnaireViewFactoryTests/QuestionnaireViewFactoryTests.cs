using System;
using DocumentFormat.OpenXml.Office2010.Excel;
using Main.Core.Documents;
using Moq;
using NUnit.Framework;
using WB.Core.BoundedContexts.Designer.MembershipProvider;
using WB.Core.BoundedContexts.Designer.Services;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.ChangeHistory;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.QuestionnaireList;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.PlainStorage;

namespace WB.Tests.Unit.Designer.BoundedContexts.Designer.QuestionnaireViewFactoryTests
{
    [TestFixture]
    internal class QuestionnaireViewFactoryTests
    {
        [Test]
        public void When_opening_questionnaire_from_public_folder_Then_should_return_allow_flag()
        {
            Guid questionnaireId = Guid.NewGuid();
            Guid userId = Guid.NewGuid();
            var questionnaireDocument = Create.QuestionnaireDocument();
            var listItem = Create.QuestionnaireListViewItem(isPublic: true, id: questionnaireId);

            var questionnaireStorage = Mock.Of<IDesignerQuestionnaireStorage>(
                q => q.Get(questionnaireId) == questionnaireDocument);

            var inMemoryDbContext = Create.InMemoryDbContext();
            inMemoryDbContext.Questionnaires.Add(listItem);
            inMemoryDbContext.SaveChanges();

            var factory = new QuestionnaireViewFactory(questionnaireStorage, inMemoryDbContext);

            QuestionnaireRevision questionnaireRevision = new QuestionnaireRevision(questionnaireId);
            var result = factory.HasUserAccessToQuestionnaire(questionnaireRevision, userId);

            Assert.True(result);
        }
        
        [Test]
        public void When_owner_try_opening_deleted_questionnaire_by_revision_imported_to_hq()
        {
            Guid questionnaireId = Guid.NewGuid();
            Guid revision = Guid.NewGuid();
            Guid userId = Guid.NewGuid();
            var questionnaireDocument = Create.QuestionnaireDocument(userId: userId);
            questionnaireDocument.IsDeleted = true;

            var listItem = Create.QuestionnaireListViewItem(isPublic: true, id: questionnaireId);

            var questionnaireStorage = Mock.Of<IDesignerQuestionnaireStorage>(
                q => q.Get(questionnaireId) == questionnaireDocument);

            var changeRecord = Create.QuestionnaireChangeRecord(revision.FormatGuid(), questionnaireId.FormatGuid(),
                QuestionnaireActionType.ImportToHq);
            
            var inMemoryDbContext = Create.InMemoryDbContext();
            inMemoryDbContext.Questionnaires.Add(listItem);
            inMemoryDbContext.QuestionnaireChangeRecords.Add(changeRecord);
            inMemoryDbContext.SaveChanges();

            var factory = new QuestionnaireViewFactory(questionnaireStorage, inMemoryDbContext);

            QuestionnaireRevision questionnaireRevision = new QuestionnaireRevision(questionnaireId, revision: revision);
            var result = factory.HasUserAccessToQuestionnaire(questionnaireRevision, userId);

            Assert.True(result);
        }
    }
}
