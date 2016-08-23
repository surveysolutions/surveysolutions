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
    internal class when_verifying_questionnaire_with_long_fixed_roster_and_it_is_nested_intro_other_roster : QuestionnaireVerifierTestsContext
    {
        private Establish context = () =>
        {
            var titles = new List<FixedRosterTitle>();
            for (int i = 1; i <= 80; i++)
            {
                titles.Add(Create.FixedRosterTitle(i, "Roster " + i));
            }

            questionnaire = Create.QuestionnaireDocument(children: new IComposite[]
            {
                Create.Chapter(children: new IComposite[]
                {
                    Create.Roster(roster2Id, fixedRosterTitles: new [] {Create.FixedRosterTitle(1, "Hello")}, rosterSizeSourceType: RosterSizeSourceType.FixedTitles, children: new IComposite[]
                    {
                        Create.TextListQuestion(questionId, maxAnswerCount: 80),
                        Create.Roster(rosterId, fixedRosterTitles: titles.ToArray(), rosterSizeSourceType: RosterSizeSourceType.FixedTitles)
                    })
                })
            });

            verifier = CreateQuestionnaireVerifier();
        };

        Because of = () =>
            verificationMessages = verifier.CheckForErrors(questionnaire);

        It should_return_contain_error_WB0081 = () =>
            verificationMessages.ShouldContainError("WB0081");

        It should_return_message_with_level_general = () =>
            verificationMessages.GetError("WB0081").MessageLevel.ShouldEqual(VerificationMessageLevel.General);

        It should_return_message_with_reference_on_roster = () =>
            verificationMessages.GetError("WB0081").References.Single().Id.ShouldEqual(rosterId);

        private static IEnumerable<QuestionnaireVerificationMessage> verificationMessages;
        private static QuestionnaireVerifier verifier;
        private static QuestionnaireDocument questionnaire;
        private static readonly Guid questionId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
        private static readonly Guid rosterId = Guid.Parse("DDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDD");
        private static readonly Guid roster2Id = Guid.Parse("BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB");
    }
}