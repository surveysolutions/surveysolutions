using System;
using System.Collections.Generic;
using System.Linq;
using Machine.Specifications;
using Main.Core.Documents;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using Main.Core.Entities.SubEntities.Question;
using WB.Core.BoundedContexts.Designer.Implementation.Services;
using WB.Core.BoundedContexts.Designer.ValueObjects;
using WB.Core.GenericSubdomains.Portable;
using QuestionnaireVerifier = WB.Core.BoundedContexts.Designer.Verifier.QuestionnaireVerifier;

namespace WB.Tests.Unit.Designer.BoundedContexts.Designer.QuestionnaireVerificationTests
{
    internal class when_verifying_questionnaire_with_2_rosters_inside_roster_and_second_roster_has_roster_size_question_from_first_roster : QuestionnaireVerifierTestsContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            Guid rosterSizeQuestionId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
            Guid rosterSizeQuestionForChildRoster1Id = Guid.Parse("DDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDD");
            questionnaire = CreateQuestionnaireDocument(new IComposite[]
            {
                Create.NumericIntegerQuestion(
                    id: rosterSizeQuestionId,
                    variable: "var1"
                ),
                Create.NumericIntegerQuestion
                (
                    rosterSizeQuestionForChildRoster1Id,
                    variable: "var2"
                ),
                new Group
                {
                    IsRoster = true,
                    VariableName = "a",
                    RosterSizeQuestionId = rosterSizeQuestionId,
                    Children = new List<IComposite>
                    {
                        new Group
                        {
                            IsRoster = true,
                            VariableName = "b",
                            RosterSizeQuestionId = rosterSizeQuestionForChildRoster1Id,
                            Children = new List<IComposite>
                            {
                                Create.NumericIntegerQuestion
                                (
                                    id: rosterSizeQuestionWithInvalidRosterLevelId,
                                    variable: "var3"
                                )
                            }.ToReadOnlyCollection()
                        },
                        new Group
                        {
                            PublicKey = groupWithInvalidRosterSizeQuestionId,
                            IsRoster = true,
                            VariableName = "c",
                            RosterSizeQuestionId = rosterSizeQuestionWithInvalidRosterLevelId
                        }
                    }.ToReadOnlyCollection()
                }
            });

            verifier = CreateQuestionnaireVerifier();
            BecauseOf();
        }

        private void BecauseOf() =>
            verificationMessages = verifier.CheckForErrors(Create.QuestionnaireView(questionnaire));

        [NUnit.Framework.Test] public void should_return_1_message () =>
            verificationMessages.Count().ShouldEqual(1);

        [NUnit.Framework.Test] public void should_return_message_with_code__WB0054__ () =>
            verificationMessages.Single().Code.ShouldEqual("WB0054");

        [NUnit.Framework.Test] public void should_return_message_with_level_general () =>
            verificationMessages.Single().MessageLevel.ShouldEqual(VerificationMessageLevel.General);

        [NUnit.Framework.Test] public void should_return_message_with_2_references () =>
            verificationMessages.Single().References.Count().ShouldEqual(2);

        [NUnit.Framework.Test] public void should_return_first_message_reference_with_type_Roster () =>
            verificationMessages.Single().References.ElementAt(0).Type.ShouldEqual(QuestionnaireVerificationReferenceType.Roster);

        [NUnit.Framework.Test] public void should_return_first_message_reference_with_id_of_rosterId () =>
            verificationMessages.Single().References.ElementAt(0).Id.ShouldEqual(groupWithInvalidRosterSizeQuestionId);

        [NUnit.Framework.Test] public void should_return_second_message_reference_with_type_question () =>
            verificationMessages.Single().References.ElementAt(1).Type.ShouldEqual(QuestionnaireVerificationReferenceType.Question);

        [NUnit.Framework.Test] public void should_return_second_message_reference_with_id_of_rosterSizeQuestionWithInvalidRosterLevelId () =>
            verificationMessages.Single().References.ElementAt(1).Id.ShouldEqual(rosterSizeQuestionWithInvalidRosterLevelId);

        private static IEnumerable<QuestionnaireVerificationMessage> verificationMessages;
        private static QuestionnaireVerifier verifier;
        private static QuestionnaireDocument questionnaire;
        private static Guid groupWithInvalidRosterSizeQuestionId = Guid.Parse("BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB");
        private static Guid rosterSizeQuestionWithInvalidRosterLevelId = Guid.Parse("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC");
    }
}
