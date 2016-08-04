using System;
using System.Collections.Generic;
using System.Linq;
using Machine.Specifications;
using Main.Core.Documents;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using WB.Core.BoundedContexts.Designer.Implementation.Services;
using WB.Core.BoundedContexts.Designer.ValueObjects;

namespace WB.Tests.Unit.Designer.BoundedContexts.Designer.QuestionnaireVerificationTests.LongRosters
{
    internal class when_verifying_questionnaire_with_long_list_roster_with_missing_max_value : QuestionnaireVerifierTestsContext
    {
        private Establish context = () =>
        {
            questionnaire = Create.QuestionnaireDocument(children: new IComposite[]
            {
                Create.Chapter(children: new IComposite[]
                {
                    Create.TextListQuestion(questionId, maxAnswerCount: null),
                    Create.Roster(rosterId, rosterSizeQuestionId: questionId, rosterSizeSourceType: RosterSizeSourceType.Question)
                })
            });

            verifier = CreateQuestionnaireVerifier();
        };

        Because of = () =>
            verificationMessages = verifier.CheckForErrors(questionnaire);

        It should_return_contain_error_WB0093 = () =>
            verificationMessages.ShouldContainError("WB0093");

        It should_return_message_with_level_general = () =>
            verificationMessages.GetError("WB0093").MessageLevel.ShouldEqual(VerificationMessageLevel.General);

        It should_return_message_with_reference_on_question = () =>
            verificationMessages.GetError("WB0093").References.Single().Id.ShouldEqual(questionId);

        private static IEnumerable<QuestionnaireVerificationMessage> verificationMessages;
        private static QuestionnaireVerifier verifier;
        private static QuestionnaireDocument questionnaire;
        private static readonly Guid questionId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
        private static readonly Guid rosterId = Guid.Parse("DDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDD");
    }
}