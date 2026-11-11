using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using WB.UI.Designer.Controllers;
using WB.UI.Shared.Web.Attributes;

namespace WB.Tests.Unit.Designer.Applications.QuestionnaireControllerTests
{
    [TestFixture]
    [TestOf(typeof(QuestionnaireController))]
    internal class QuestionnaireControllerTransactionAttributesTests
    {
        [Test]
        public void should_mark_update_anonymous_questionnaire_settings_with_no_transaction()
        {
            typeof(QuestionnaireController)
                .GetMethod(nameof(QuestionnaireController.UpdateAnonymousQuestionnaireSettings))!
                .GetCustomAttributes(typeof(NoTransactionAttribute), inherit: true)
                .OfType<NoTransactionAttribute>()
                .Should().ContainSingle();
        }
    }
}
