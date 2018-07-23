using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Main.Core.Documents;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using Main.Core.Entities.SubEntities.Question;
using WB.Core.BoundedContexts.Designer.ValueObjects;
using WB.Core.GenericSubdomains.Portable;
using QuestionnaireVerifier = WB.Core.BoundedContexts.Designer.Verifier.QuestionnaireVerifier;

namespace WB.Tests.Unit.Designer.QuestionnaireVerificationTests
{
    class when_verifying_questionnaire_with_question_that_has_two_question_with_same_variable_names : QuestionnaireVerifierTestsContext
    {

        [NUnit.Framework.OneTimeSetUp] public void context () {
            questionnaire = CreateQuestionnaireDocumentWithOneChapter(new Group("Chapter")
            {
                Children = new List<IComposite>()
                {
                    new NumericQuestion("first")
                    {
                        PublicKey = firstQuestionId,
                        StataExportCaption = variableName
                    },
                    new NumericQuestion("second")
                    {
                        PublicKey = secondQuestionId,
                        StataExportCaption = variableName
                    }
                }.ToReadOnlyCollection()
            });

            verifier = CreateQuestionnaireVerifier();
            BecauseOf();
        }

        private void BecauseOf() =>
            verificationMessages = verifier.CheckForErrors(Create.QuestionnaireView(questionnaire));

        [NUnit.Framework.Test] public void should_return_WB0026_error () =>
            verificationMessages.ShouldContainCritical("WB0026");

        [NUnit.Framework.Test] public void should_return_message_with_level_critical () =>
            verificationMessages.GetCritical("WB0026").MessageLevel.Should().Be(VerificationMessageLevel.Critical);

        [NUnit.Framework.Test] public void should_return_message_with_1_reference () =>
            verificationMessages.GetCritical("WB0026").References.Count().Should().Be(2);

        [NUnit.Framework.Test] public void should_return_message_reference_with_type_Question () =>
            verificationMessages.GetCritical("WB0026").References.Should().OnlyContain(reference => reference.Type == QuestionnaireVerificationReferenceType.Question);

        [NUnit.Framework.Test] public void should_return_message_reference_with_first_and_second_question_Id () =>
            verificationMessages.GetCritical("WB0026").References.Select(x => x.Id).Should().Contain(new []{firstQuestionId, secondQuestionId});

        private static QuestionnaireVerifier verifier;
        private static QuestionnaireDocument questionnaire;

        private static IEnumerable<QuestionnaireVerificationMessage> verificationMessages;

        private static Guid firstQuestionId = Guid.Parse("a1111111111111111111111111111111");
        private static Guid secondQuestionId = Guid.Parse("b1111111111111111111111111111111");
        private static string variableName = "same";

    }
}
