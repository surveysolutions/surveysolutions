using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Main.Core.Documents;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using WB.Core.BoundedContexts.Designer.ValueObjects;
using QuestionnaireVerifier = WB.Core.BoundedContexts.Designer.Verifier.QuestionnaireVerifier;

namespace WB.Tests.Unit.Designer.QuestionnaireVerificationTests.CascadingDropdownTests
{
    internal class when_cascading_question_from_roster_references_parent_from_parent_roster : QuestionnaireVerifierTestsContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            questionnaire = Create.QuestionnaireDocument(
                children: Create.Roster(
                    variable: "roster1",
                    rosterType: RosterSizeSourceType.FixedTitles,
                    fixedTitles: new[] { "a", "b" },
                    children: new List<IComposite>
                    {
                        Create.SingleQuestion(parentSingleOptionQuestionId, "q1", options: new List<Answer>
                        {
                            Create.Option(value: "1", text: "parent option 1"),
                            Create.Option(value: "2", text: "parent option 2")
                        }),
                        Create.Roster(
                            variable: "roster2",
                            rosterType: RosterSizeSourceType.FixedTitles,
                            fixedTitles: new[] { "a", "b" },
                            children: new List<IComposite>
                            {
                                Create.SingleQuestion(childCascadedComboboxId, "q2", cascadeFromQuestionId: parentSingleOptionQuestionId,
                                    options:
                                        new List<Answer>
                                        {
                                            Create.Option(value: "1", text: "child 1 for parent option 1", parentValue: "1"),
                                            Create.Option(value: "3", text: "child 1 for parent option 2", parentValue: "2")
                                        }
                                    )
                            })
                    })
                );
            verifier = CreateQuestionnaireVerifier();
            BecauseOf();
        }

        private void BecauseOf() => 
            verificationErrors = verifier.CheckForErrors(Create.QuestionnaireView(questionnaire));

        [NUnit.Framework.Test] public void should_not_return_verification_errors () => 
            verificationErrors.Count().Should().Be(0);

        static QuestionnaireDocument questionnaire;
        static Guid childCascadedComboboxId= Guid.Parse("99999999999999999999999999999999");
        static Guid parentSingleOptionQuestionId = Guid.Parse("88888888888888888888888888888888");
        static QuestionnaireVerifier verifier;
        static IEnumerable<QuestionnaireVerificationMessage> verificationErrors;
    }
}