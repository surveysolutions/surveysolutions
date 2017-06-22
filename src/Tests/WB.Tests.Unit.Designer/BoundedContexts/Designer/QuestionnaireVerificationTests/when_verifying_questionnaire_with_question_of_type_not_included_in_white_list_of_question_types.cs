using System;
using System.Collections.Generic;
using System.Linq;
using Machine.Specifications;
using Main.Core.Documents;
using Main.Core.Entities.SubEntities;
using Main.Core.Entities.SubEntities.Question;
using WB.Core.BoundedContexts.Designer.Implementation.Services;
using WB.Core.BoundedContexts.Designer.ValueObjects;

namespace WB.Tests.Unit.Designer.BoundedContexts.Designer.QuestionnaireVerificationTests
{
    internal class when_verifying_questionnaire_with_question_of_type_not_included_in_white_list_of_question_types:
        QuestionnaireVerifierTestsContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {

            questionId = Guid.Parse("1111CCCCCCCCCCCCCCCCCCCCCCCCCCCC");

            questionnaire = CreateQuestionnaireDocumentWithOneChapter(
                new SingleQuestion("test")
                {
                    PublicKey = questionId,
                    QuestionType = QuestionType.YesNo,
                    StataExportCaption = "var1",
                    Answers =
                        new List<Answer>
                        {
                            new Answer() { AnswerValue = "1", AnswerText = "1" },
                            new Answer() { AnswerValue = "2", AnswerText = "2" }
                        }
                }
                );

            verifier = CreateQuestionnaireVerifier();
            BecauseOf();
        }

        private void BecauseOf() =>
            verificationMessages = verifier.CheckForErrors(Create.QuestionnaireView(questionnaire));

        [NUnit.Framework.Test] public void should_return_1_message () =>
            verificationMessages.Count().ShouldEqual(1);

        [NUnit.Framework.Test] public void should_return_message_with_level_general () =>
            verificationMessages.Single().MessageLevel.ShouldEqual(VerificationMessageLevel.General);

        [NUnit.Framework.Test] public void should_return_messages_each_with_code__WB0002__ () =>
            verificationMessages.ShouldEachConformTo(error
                => error.Code == "WB0066");

        [NUnit.Framework.Test] public void should_return_messages_each_having_single_reference () =>
            verificationMessages.ShouldEachConformTo(error
                => error.References.Count() == 1);

        [NUnit.Framework.Test] public void should_return_messages_each_referencing_question () =>
            verificationMessages.ShouldEachConformTo(error
                => error.References.Single().Type == QuestionnaireVerificationReferenceType.Question);

        [NUnit.Framework.Test] public void should_return_message_referencing_first_incorrect_question () =>
            verificationMessages.ShouldContain(error
                => error.References.Single().Id == questionId);

        private static IEnumerable<QuestionnaireVerificationMessage> verificationMessages;
        private static QuestionnaireVerifier verifier;
        private static QuestionnaireDocument questionnaire;
        private static Guid questionId;
    }
}
