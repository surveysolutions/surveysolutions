using System;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;
using WB.Core.BoundedContexts.Designer.Implementation.Services.LookupTableService;
using WB.Core.BoundedContexts.Designer.Services;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.ChangeHistory;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.SharedKernels.SurveySolutions.ReusableCategories;
using WB.UI.Designer.Controllers.Api.Assistant;
using WB.UI.Designer.Services;

namespace WB.Tests.Unit.Designer.Api.Assistant
{
    [TestFixture]
    [TestOf(typeof(QuestionnairesController))]
    public class QuestionnairesControllerTests
    {
        [Test]
        public void GetLookupTableHeaders_when_lookup_table_is_missing_should_return_not_found()
        {
            var lookupTableService = new Mock<ILookupTableService>();
            lookupTableService
                .Setup(service => service.GetLookupTableContentFile(
                    It.IsAny<QuestionnaireRevision>(), It.IsAny<Guid>()))
                .Throws(new ArgumentException());

            var controller = CreateController(lookupTableService.Object);

            var result = controller.GetLookupTableHeaders(
                new QuestionnaireRevision(Guid.NewGuid(), version: 1),
                Guid.NewGuid());

            Assert.That(result, Is.InstanceOf<NotFoundResult>());
        }

        [Test]
        public void GetLookupTableHeaders_when_content_is_tab_separated_should_return_trimmed_headers_in_order()
        {
            var controller = CreateControllerWithContent("\r\n rowcode\t price \tname\r\n1\t2\t3\r\n");

            var result = controller.GetLookupTableHeaders(
                new QuestionnaireRevision(Guid.NewGuid(), version: 1),
                Guid.NewGuid());

            Assert.That(result, Is.InstanceOf<OkObjectResult>());
            Assert.That(((OkObjectResult)result).Value, Is.EqualTo(new[] { "rowcode", "price", "name" }));
        }

        [Test]
        public void GetLookupTableHeaders_when_content_is_comma_separated_should_return_headers()
        {
            var controller = CreateControllerWithContent("rowcode,price\n1,2\n");

            var result = controller.GetLookupTableHeaders(
                new QuestionnaireRevision(Guid.NewGuid(), version: 1),
                Guid.NewGuid());

            Assert.That(result, Is.InstanceOf<OkObjectResult>());
            Assert.That(((OkObjectResult)result).Value, Is.EqualTo(new[] { "rowcode", "price" }));
        }

        [Test]
        public void GetLookupTableHeaders_when_content_is_empty_should_return_empty_headers()
        {
            var controller = CreateControllerWithContent("\r\n \r\n");

            var result = controller.GetLookupTableHeaders(
                new QuestionnaireRevision(Guid.NewGuid(), version: 1),
                Guid.NewGuid());

            Assert.That(result, Is.InstanceOf<OkObjectResult>());
            Assert.That(((OkObjectResult)result).Value, Is.Empty);
        }

        private static QuestionnairesController CreateControllerWithContent(string content)
        {
            var lookupTableService = new Mock<ILookupTableService>();
            lookupTableService
                .Setup(service => service.GetLookupTableContentFile(
                    It.IsAny<QuestionnaireRevision>(), It.IsAny<Guid>()))
                .Returns(new LookupTableContentFile("lookup.tab", Encoding.UTF8.GetBytes(content)));

            return CreateController(lookupTableService.Object);
        }

        private static QuestionnairesController CreateController(ILookupTableService lookupTableService) =>
            new QuestionnairesController(
                Mock.Of<IQuestionnaireViewFactory>(),
                Mock.Of<IQuestionnaireDocumentTransformer>(),
                Mock.Of<IReusableCategoriesService>(),
                lookupTableService,
                Mock.Of<ISerializer>());
    }
}
