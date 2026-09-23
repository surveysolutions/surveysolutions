using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using WB.UI.Designer.Controllers;
using WB.UI.Shared.Web.Attributes;

namespace WB.Tests.Unit.Designer.Applications.QuestionnaireControllerTests
{
    [TestFixture]
    [TestOf(typeof(QuestionnaireController))]
    internal class when_deleting_questionnaire : QuestionnaireControllerTestContext
    {
        [Test]
        public void should_skip_request_transaction()
        {
            var deleteAction = typeof(QuestionnaireController)
                .GetMethods()
                .Single(method => method.Name == nameof(QuestionnaireController.Delete));

            deleteAction.GetCustomAttributes(typeof(NoTransactionAttribute), inherit: true)
                .Should().ContainSingle();
        }
    }
}
