
using System;
using System.Linq;
using NUnit.Framework;
using WB.Core.BoundedContexts.Designer.ValueObjects;
using WB.Core.BoundedContexts.Designer.Verifier;

namespace WB.Tests.Unit.Designer.QuestionnaireVerificationTests.CascadingDropdownTests
{
    [TestOf(typeof(QuestionnaireVerifier))]
    internal partial class Verification : QuestionnaireVerifierTestsContext
    {
        [Test]
        public void when_macro_has_empty_content()
        {
            //arrange
            Guid macroId = Guid.Parse("11111111111111111111111111111111");

            var questionnaire = Create.QuestionnaireDocument(Guid.NewGuid(), Create.TextQuestion(variable: "var"));
            questionnaire.Macros.Add(macroId, Create.Macro("macroname"));

            var verifier = CreateQuestionnaireVerifier();

            //act
            var verificationMessages = verifier.CheckForErrors(Create.QuestionnaireView(questionnaire));

            //assert
            Assert.AreEqual(verificationMessages.Count(), 1);
            Assert.AreEqual(verificationMessages.Single().Code, "WB0271");
            Assert.AreEqual(verificationMessages.Single().References.Count, 1);
            Assert.That(verificationMessages.Single().References.Select(x=>x.Type), Is.All.EqualTo(QuestionnaireVerificationReferenceType.Macro));
            Assert.AreEqual(verificationMessages.Single().References.ElementAt(0).Id, macroId);
        }
    }
}
