using NUnit.Framework;
using WB.Core.BoundedContexts.Designer.Verifier;

namespace WB.Tests.Unit.Designer.QuestionnaireVerificationTests
{
    [TestOf(typeof(CategoriesVerifications))]
    internal class CategoriesVerificationsTests
    {
        [TestCase("aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa", "WB0289")]
        [TestCase("variable_", "WB0290")]
        [TestCase("vari__able", "WB0291")]
        public void when_name_is_invalid(string name, string errorCode)
            => Create.QuestionnaireDocument("v", categories: new[] {Create.Categories(name: name)}).ExpectError(errorCode);

        [TestCase(" ", "WB0292")]
        [TestCase("abstract", "WB0293")]
        [TestCase("variableЙФЪ", "WB0294")]
        [TestCase("_variable", "WB0295")]
        [TestCase("1variable", "WB0295")]
        public void when_name_is_critically_invalid(string name, string errorCode)
            => Create.QuestionnaireDocument("v", categories: new[] {Create.Categories(name: name)}).ExpectCritical(errorCode);
    }
}
