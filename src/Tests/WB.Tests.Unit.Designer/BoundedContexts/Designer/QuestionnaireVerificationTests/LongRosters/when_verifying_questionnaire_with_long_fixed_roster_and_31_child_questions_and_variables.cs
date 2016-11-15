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
    internal class when_verifying_questionnaire_with_long_fixed_roster_and_31_child_questions_and_variables : QuestionnaireVerifierTestsContext
    {
        private Establish context = () =>
        {
            var childQuestions = new List<IComposite>();
            for (int i = 1; i <= Constants.MaxAmountOfItemsInLongRoster; i++)
            {
                childQuestions.Add(Create.TextQuestion());
            }
            childQuestions.Add(Create.Variable());

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
            verificationMessages = verifier.CheckForErrors(Create.QuestionnaireView(questionnaire));

        It should_not_contain_error_WB0068 = () =>
            verificationMessages.ShouldNotContain("WB0068");


        private static IEnumerable<QuestionnaireVerificationMessage> verificationMessages;
        private static QuestionnaireVerifier verifier;
        private static QuestionnaireDocument questionnaire;
        private static readonly Guid rosterId = Guid.Parse("DDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDD");
    }
}