using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Main.Core.Documents;
using Main.Core.Entities.Composite;
using WB.Core.BoundedContexts.Designer.ValueObjects;
using QuestionnaireVerifier = WB.Core.BoundedContexts.Designer.Verifier.QuestionnaireVerifier;

namespace WB.Tests.Unit.Designer.QuestionnaireVerificationTests
{
    internal class when_group_without_questions_added : QuestionnaireVerifierTestsContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            groupId = Guid.NewGuid();
            questionnaire = Create.QuestionnaireDocumentWithOneChapter(children: new IComposite[]
            {
                Create.TextQuestion(),
                Create.Group(groupId: groupId)
            });
            verifier = CreateQuestionnaireVerifier();
            BecauseOf();
        }

        private void BecauseOf() => errors = verifier.CompileAndVerify(Create.QuestionnaireView(questionnaire),
            null, out string _);

        [NUnit.Framework.Test] public void should_return_warning () => errors.Where(x => x.MessageLevel == VerificationMessageLevel.Warning).Should().NotBeEmpty();

        [NUnit.Framework.Test] public void should_return_WB0202_warning_with_reference_to_empty_group () 
        {
            var warnings = errors
                .Where(x => x.MessageLevel == VerificationMessageLevel.Warning)
                .Where(x => x.Code == "WB0202");

            warnings.Should().NotBeEmpty();
            warnings.Second().References.First().Id.Should().Be(groupId);
        }


        static QuestionnaireDocument questionnaire;
        static QuestionnaireVerifier verifier;
        static IEnumerable<QuestionnaireVerificationMessage> errors;
        static Guid groupId;
    }
}
