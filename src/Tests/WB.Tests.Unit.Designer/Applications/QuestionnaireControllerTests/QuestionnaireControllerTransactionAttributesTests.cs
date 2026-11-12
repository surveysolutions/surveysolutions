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
        public void should_use_transaction_filter_for_anonymous_sharing_actions()
        {
            new[]
                {
                    nameof(QuestionnaireController.UpdateAnonymousQuestionnaireSettings),
                    nameof(QuestionnaireController.RegenerateAnonymousQuestionnaireLink)
                }
                .Select(action => typeof(QuestionnaireController).GetMethod(action)!)
                .Should()
                .OnlyContain(action => !action.GetCustomAttributes(typeof(NoTransactionAttribute), inherit: true)
                    .OfType<NoTransactionAttribute>()
                    .Any());
        }
    }
}
