using System;
using System.Collections.Generic;
using Main.Core.Documents;
using Moq;
using NUnit.Framework;
using WB.Core.BoundedContexts.Designer.Implementation.Services.LookupTableService;
using WB.Core.BoundedContexts.Designer.Services;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.ChangeHistory;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.SurveySolutions.Documents;

namespace WB.Tests.Unit.Designer.BoundedContexts.Designer.LookupTableServiceTests
{
    internal class when_getting_lookup_table_headers
    {
        private static readonly Guid questionnaireId = Guid.Parse("11111111111111111111111111111111");
        private static readonly Guid lookupTableId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");

        [Test]
        public void should_return_rowcode_followed_by_variable_names()
        {
            var service = CreateService(Create.LookupTableContent(new[] { "price", "name" }));

            var headers = service.GetLookupTableHeaders(new QuestionnaireRevision(questionnaireId), lookupTableId);

            Assert.That(headers, Is.EqualTo(new[] { "rowcode", "price", "name" }));
        }

        [Test]
        public void should_return_null_when_content_is_not_stored()
        {
            var service = CreateService(null);

            var headers = service.GetLookupTableHeaders(new QuestionnaireRevision(questionnaireId), lookupTableId);

            Assert.That(headers, Is.Null);
        }

        [Test]
        public void should_throw_when_lookup_table_is_missing()
        {
            var service = CreateService(null);

            Assert.Throws<ArgumentException>(() =>
                service.GetLookupTableHeaders(new QuestionnaireRevision(questionnaireId), Guid.NewGuid()));
        }

        private static LookupTableService CreateService(LookupTableContent content)
        {
            var questionnaire = new QuestionnaireDocument
            {
                PublicKey = questionnaireId,
                LookupTables = new Dictionary<Guid, LookupTable> { { lookupTableId, new LookupTable() } }
            };

            var documentStorage = new Mock<IDesignerQuestionnaireStorage>();
            documentStorage.Setup(x => x.Get(It.IsAny<QuestionnaireRevision>())).Returns(questionnaire);

            var contentStorage = new Mock<IPlainKeyValueStorage<LookupTableContent>>();
            contentStorage.Setup(x => x.GetById(It.IsAny<string>())).Returns(content);

            return Create.LookupTableService(contentStorage.Object, documentStorage.Object);
        }
    }
}
