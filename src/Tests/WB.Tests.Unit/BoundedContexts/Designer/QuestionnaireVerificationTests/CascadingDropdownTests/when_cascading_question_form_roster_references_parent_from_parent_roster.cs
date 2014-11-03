using System;
using System.Collections.Generic;
using System.Linq;
using Machine.Specifications;
using Main.Core.Documents;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using WB.Core.BoundedContexts.Designer.Implementation.Services;
using WB.Core.BoundedContexts.Designer.ValueObjects;

namespace WB.Tests.Unit.BoundedContexts.Designer.QuestionnaireVerificationTests.CascadingDropdownTests
{
    internal class when_cascading_question_form_roster_references_parent_from_parent_roster : QuestionnaireVerifierTestsContext
    {
        Establish context = () =>
        {
           questionnaire = Create.QuestionnaireDocument(Guid.NewGuid(),

                Create.Roster(Guid.NewGuid(),
                    variable: "roster1",
                    rosterSizeSourceType: RosterSizeSourceType.FixedTitles,
                    fixedTitles: new[] { "a", "b" },
                    children: new List<IComposite>
                    {
                        Create.SingleQuestion(parentSingleOptionQuestionId, "q1", options: new List<Answer>
                        {
                            Create.Option(text: "parent option 1", value: "1"),
                            Create.Option(text: "parent option 2", value: "2")
                        }),
                        Create.Roster(Guid.NewGuid(),
                            variable: "roster2",
                            rosterSizeSourceType: RosterSizeSourceType.FixedTitles,
                            fixedTitles: new[] { "a", "b" },
                            children: new List<IComposite>
                            {
                                Create.SingleQuestion(childCascadedComboboxId, "q2", cascadeFromQuestionId: parentSingleOptionQuestionId,
                                    options:
                                        new List<Answer>
                                        {
                                            Create.Option(text: "child 1 for parent option 1", value: "1", parentValue: "1"),
                                            Create.Option(text: "child 1 for parent option 2", value: "3", parentValue: "2")
                                        }
                                    )
                            })
                    })
                );
            verifier = CreateQuestionnaireVerifier();
        };

        Because of = () => 
            verificationErrors = verifier.Verify(questionnaire);

        It should_not_return_verification_errors = () => 
            verificationErrors.Count().ShouldEqual(0);

        static QuestionnaireDocument questionnaire;
        static Guid childCascadedComboboxId= Guid.Parse("99999999999999999999999999999999");
        static Guid parentSingleOptionQuestionId = Guid.Parse("88888888888888888888888888888888");
        static QuestionnaireVerifier verifier;
        static IEnumerable<QuestionnaireVerificationError> verificationErrors;
    }
}