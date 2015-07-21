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

namespace WB.Tests.Unit.BoundedContexts.Designer.QuestionnaireVerificationTests
{
    internal class when_verifying_questionnaire_with_roster_by_question_that_have_roster_title_question_in_roster_with_other_scope : QuestionnaireVerifierTestsContext
    {
        Establish context = () =>
        {
            questionnaire = CreateQuestionnaireDocumentWithOneChapter(
                new NumericQuestion("rosterSizeQuestion1")
                {
                    PublicKey = rosterSizeQuestion1Id,
                    StataExportCaption = "var1",
                    IsInteger = true
                },
                new NumericQuestion("rosterSizeQuestion2")
                {
                    PublicKey = rosterSizeQuestion2Id,
                    StataExportCaption = "var2",
                    IsInteger = true
                },
                new Group
                {
                    Title = "Roster 1. Triggered by rosterSizeQuestion2",
                    PublicKey = roster1Id,
                    IsRoster = true,
                    VariableName = "a",
                    RosterSizeSource = RosterSizeSourceType.Question,
                    RosterSizeQuestionId = rosterSizeQuestion2Id,
                    RosterTitleQuestionId = rosterTitleQuestionId,
                    Children = new List<IComposite>()
                    {
                        new TextQuestion("rosterTitleQuestion")
                        {
                            StataExportCaption = "var3",
                            PublicKey = rosterTitleQuestionId
                        }
                    }
                },
                new Group
                {
                    Title = "Roster 2. Triggered by rosterSizeQuestion1",
                    PublicKey = roster2Id,
                    IsRoster = true,
                    VariableName = "b",
                    RosterSizeSource = RosterSizeSourceType.Question,
                    RosterSizeQuestionId = rosterSizeQuestion1Id,
                    RosterTitleQuestionId = rosterTitleQuestionId
                }
                );

            verifier = CreateQuestionnaireVerifier();
        };

        Because of = () =>
            resultErrors = verifier.Verify(questionnaire);

        It should_return_1_error = () =>
            resultErrors.Count().ShouldEqual(1);

        It should_return_error_with_code__WB0035__ = () =>
            resultErrors.Single().Code.ShouldEqual("WB0035");

        It should_return_error_with_1_references = () =>
            resultErrors.Single().References.Count().ShouldEqual(1);

        It should_return_error_reference_with_type_group = () =>
            resultErrors.Single().References.Single().Type.ShouldEqual(QuestionnaireVerificationReferenceType.Group);

        It should_return_error_reference_with_id_of_roster2Id = () =>
            resultErrors.Single().References.First().Id.ShouldEqual(roster2Id);

        private static IEnumerable<QuestionnaireVerificationError> resultErrors;
        private static QuestionnaireVerifier verifier;
        private static QuestionnaireDocument questionnaire;
        private static Guid roster1Id = Guid.Parse("10000000000000000000000000000000");
        private static Guid roster2Id = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
        private static Guid rosterSizeQuestion1Id = Guid.Parse("13333333333333333333333333333333");
        private static Guid rosterSizeQuestion2Id = Guid.Parse("22222222222222222222222222222222");
        private static Guid rosterTitleQuestionId = Guid.Parse("11333333333333333333333333333333");
    }
}