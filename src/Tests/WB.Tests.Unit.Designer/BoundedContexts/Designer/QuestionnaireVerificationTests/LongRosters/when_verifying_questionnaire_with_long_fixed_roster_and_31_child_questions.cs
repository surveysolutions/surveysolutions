using System;
using System.Collections.Generic;
using System.Linq;
using Machine.Specifications;
using Main.Core.Documents;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using WB.Core.BoundedContexts.Designer.Implementation.Services;
using WB.Core.BoundedContexts.Designer.ValueObjects;
using WB.Core.SharedKernels.SurveySolutions.Documents;

namespace WB.Tests.Unit.Designer.BoundedContexts.Designer.QuestionnaireVerificationTests.LongRosters
{
    internal class when_verifying_questionnaire_with_long_fixed_roster_and_31_child_questions : QuestionnaireVerifierTestsContext
    {
        private Establish context = () =>
        {
            var childQuestions = new List<IComposite>();
            for (int i = 1; i <= 31; i++)
            {
                childQuestions.Add(Create.TextQuestion());
            }

            var titles = new List<FixedRosterTitle>();
            for (int i = 1; i <= 80; i++)
            {
                titles.Add(Create.FixedRosterTitle(i, "Roster " + i));
            }

            questionnaire = Create.QuestionnaireDocument(children: new IComposite[]
            {
                Create.Chapter(children: new IComposite[]
                {
                    Create.Roster(rosterId, fixedRosterTitles: titles.ToArray(), rosterType: RosterSizeSourceType.FixedTitles, children: childQuestions)
                })
            });

            verifier = CreateQuestionnaireVerifier();
        };

        Because of = () =>
            verificationMessages = verifier.CheckForErrors(questionnaire);

        It should_return_contain_error_WB0068 = () =>
            verificationMessages.ShouldContainError("WB0068");

        It should_return_message_with_level_general = () =>
            verificationMessages.GetError("WB0068").MessageLevel.ShouldEqual(VerificationMessageLevel.General);

        It should_return_message_with_specified_text = () =>
            verificationMessages.GetError("WB0068").Message.ShouldEqual("Roster cannot have more than 30 child elements");

        It should_return_message_with_reference_on_roster = () =>
            verificationMessages.GetError("WB0068").References.Single().Id.ShouldEqual(rosterId);

        private static IEnumerable<QuestionnaireVerificationMessage> verificationMessages;
        private static QuestionnaireVerifier verifier;
        private static QuestionnaireDocument questionnaire;
        private static readonly Guid rosterId = Guid.Parse("DDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDD");
    }
}