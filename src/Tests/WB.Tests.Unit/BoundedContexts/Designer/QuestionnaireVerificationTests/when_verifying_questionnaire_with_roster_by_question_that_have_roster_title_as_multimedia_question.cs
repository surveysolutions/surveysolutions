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
    internal class when_verifying_questionnaire_with_roster_by_question_that_have_roster_title_as_multimedia_question : QuestionnaireVerifierTestsContext
    {
        Establish context = () =>
        {
            questionnaire = CreateQuestionnaireDocumentWithOneChapter(
                new NumericQuestion("rosterSizeQuestion1")
                {
                    PublicKey = rosterSizeQuestionId,
                    StataExportCaption = "var1",
                    IsInteger = true
                },
                new Group
                {
                    Title = "Roster 1. Triggered by rosterSizeQuestion2",
                    PublicKey = rosterId,
                    IsRoster = true,
                    VariableName = "a",
                    RosterSizeSource = RosterSizeSourceType.Question,
                    RosterSizeQuestionId = rosterSizeQuestionId,
                    RosterTitleQuestionId = rosterTitleMultimediaQuestionId,
                    Children = new List<IComposite>()
                    {
                        new MultimediaQuestion("rosterTitleQuestion")
                        {
                            StataExportCaption = "var3",
                            PublicKey = rosterTitleMultimediaQuestionId
                        }
                    }
                }
                );

            verifier = CreateQuestionnaireVerifier();
        };

        Because of = () =>
            resultErrors = verifier.Verify(questionnaire);

        It should_return_1_error = () =>
            resultErrors.Count().ShouldEqual(1);

        It should_return_error_with_code__WB0035__ = () =>
            resultErrors.Single().Code.ShouldEqual("WB0083");

        It should_return_error_with_2_references = () =>
            resultErrors.Single().References.Count().ShouldEqual(2);

        It should_return_first_error_reference_with_type_group = () =>
            resultErrors.Single().References.First().Type.ShouldEqual(QuestionnaireVerificationReferenceType.Group);

        It should_return_first_error_reference_with_id_rosterId = () =>
           resultErrors.Single().References.First().Id.ShouldEqual(rosterId);

        It should_return_second_error_reference_with_type_group = () =>
           ShouldExtensionMethods.ShouldEqual(resultErrors.Single().References.Second().Type, QuestionnaireVerificationReferenceType.Question);

        It should_return_second_error_reference_with_id_rosterTitleMultimediaQuestionId = () =>
           ShouldExtensionMethods.ShouldEqual(resultErrors.Single().References.Second().Id, rosterTitleMultimediaQuestionId);

        private static IEnumerable<QuestionnaireVerificationError> resultErrors;
        private static QuestionnaireVerifier verifier;
        private static QuestionnaireDocument questionnaire;
        private static Guid rosterId = Guid.Parse("10000000000000000000000000000000");
        private static Guid rosterSizeQuestionId = Guid.Parse("13333333333333333333333333333333");
        private static Guid rosterTitleMultimediaQuestionId = Guid.Parse("11333333333333333333333333333333");
    }
}
