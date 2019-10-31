using Main.Core.Documents;
using Moq;
using NUnit.Framework;
using System;
using WB.Core.BoundedContexts.Designer.MembershipProvider;
using WB.Core.BoundedContexts.Designer.Services;
using WB.Core.Infrastructure.PlainStorage;
using WB.Tests.Abc;

namespace WB.Tests.Unit.Designer.BoundedContexts.Designer.DesignerQuestionnaireStorageTests
{
    internal class DesignerQuestionnaireStorageTest
    {
        Mock<IQuestionnaireHistoryVersionsService> questionnaireHistoryVersionsService;
        Mock<IPlainKeyValueStorage<QuestionnaireDocument>> questionnaireDocumentReader;
        DesignerQuestionnaireStorage Subject;

        [SetUp]
        public void Setup()
        {
            this.questionnaireHistoryVersionsService = new Mock<IQuestionnaireHistoryVersionsService>();
            this.questionnaireDocumentReader = new Mock<IPlainKeyValueStorage<QuestionnaireDocument>>();

            Subject = new DesignerQuestionnaireStorage(
                questionnaireHistoryVersionsService.Object, 
                questionnaireDocumentReader.Object);
        }

        [Test]
        public void when_query_with_revision_should_query_historic_storage()
        {
            // act
            Subject.Get(Create.QuestionnaireRevision(Id.g1, Id.g2));

            // assert
            this.questionnaireHistoryVersionsService.Verify(s => s.GetByHistoryVersion(It.IsAny<Guid>()), Times.Once);
            this.questionnaireDocumentReader.Verify(s => s.GetById(It.IsAny<string>()), Times.Never);
        }

        [Test]
        public void when_query_without_revision_should_not_query_historic_storage()
        {
            // act
            Subject.Get(Create.QuestionnaireRevision(Id.g1));

            // assert
            this.questionnaireHistoryVersionsService.Verify(s => s.GetByHistoryVersion(It.IsAny<Guid>()), Times.Never);
            this.questionnaireDocumentReader.Verify(s => s.GetById(It.IsAny<string>()), Times.Once);
        }
    }
}
