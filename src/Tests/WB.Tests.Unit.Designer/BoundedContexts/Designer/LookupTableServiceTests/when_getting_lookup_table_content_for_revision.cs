using System;
using Moq;
using NUnit.Framework;
using FluentAssertions;
using WB.Core.BoundedContexts.Designer.Implementation.Services.LookupTableService;
using WB.Core.BoundedContexts.Designer.Services;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.ChangeHistory;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.SurveySolutions.Documents;

namespace WB.Tests.Unit.Designer.BoundedContexts.Designer.LookupTableServiceTests
{
    [TestFixture]
    [TestOf(typeof(LookupTableService))]
    internal class when_getting_lookup_table_content_for_revision
    {
        [Test]
        public void when_table_is_absent_should_return_null()
        {
            var revision = new QuestionnaireRevision(Guid.NewGuid(), Guid.NewGuid());
            var questionnaire = Create.QuestionnaireDocument(id: revision.QuestionnaireId);
            var documentStorage = new Mock<IDesignerQuestionnaireStorage>();
            documentStorage.Setup(x => x.Get(revision)).Returns(questionnaire);
            var service = Create.LookupTableService(documentStorage: documentStorage.Object);

            service.GetLookupTableContent(revision, Guid.NewGuid()).Should().BeNull();
        }

        [Test]
        public void when_questionnaire_is_absent_should_throw()
        {
            var revision = new QuestionnaireRevision(Guid.NewGuid(), Guid.NewGuid());
            var service = Create.LookupTableService();

            Assert.Throws<ArgumentException>(() => service.GetLookupTableContent(revision, Guid.NewGuid()));
        }

        [Test]
        public void when_table_exists_should_load_content_from_revision_document()
        {
            var questionnaireId = Guid.NewGuid();
            var revision = new QuestionnaireRevision(questionnaireId, Guid.NewGuid());
            var lookupTableId = Guid.NewGuid();
            var questionnaire = Create.QuestionnaireDocument(id: questionnaireId);
            questionnaire.LookupTables.Add(lookupTableId, new LookupTable());
            var content = new LookupTableContent(Array.Empty<string>(), Array.Empty<LookupTableRow>());
            var documentStorage = new Mock<IDesignerQuestionnaireStorage>();
            documentStorage.Setup(x => x.Get(revision)).Returns(questionnaire);
            var contentStorage = new Mock<IPlainKeyValueStorage<LookupTableContent>>();
            contentStorage.Setup(x => x.GetById(It.IsAny<string>())).Returns(content);
            var service = Create.LookupTableService(contentStorage.Object, documentStorage.Object);

            service.GetLookupTableContent(revision, lookupTableId).Should().BeSameAs(content);
            documentStorage.Verify(x => x.Get(revision), Times.Once);
        }
    }
}
