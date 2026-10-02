using System;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;
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

            var controller = new QuestionnairesController(
                Mock.Of<IQuestionnaireViewFactory>(),
                Mock.Of<IQuestionnaireDocumentTransformer>(),
                Mock.Of<IReusableCategoriesService>(),
                lookupTableService.Object,
                Mock.Of<ISerializer>());

            var result = controller.GetLookupTableHeaders(
                new QuestionnaireRevision(Guid.NewGuid(), version: 1),
                Guid.NewGuid());

            Assert.That(result, Is.InstanceOf<NotFoundResult>());
        }
    }
}
