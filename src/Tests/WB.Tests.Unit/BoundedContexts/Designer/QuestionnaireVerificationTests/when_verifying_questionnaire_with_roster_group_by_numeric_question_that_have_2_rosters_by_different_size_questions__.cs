using System;
using System.Collections.Generic;
using System.Linq;
using Machine.Specifications;
using Main.Core.Documents;
using Main.Core.Entities.SubEntities;
using Main.Core.Entities.SubEntities.Question;
using WB.Core.BoundedContexts.Designer.Implementation.Services;
using WB.Core.BoundedContexts.Designer.ValueObjects;

namespace WB.Tests.Unit.BoundedContexts.Designer.QuestionnaireVerificationTests
{
    internal class when_verifying_questionnaire_with_roster_group_by_numeric_question_that_have_2_rosters_by_different_size_question_and_1__the_same_title_question : QuestionnaireVerifierTestsContext
    {
        Establish context = () =>
        {
            questionnaire = CreateQuestionnaireDocument();
            questionnaire.Children.Add(new NumericQuestion("roster size question 1")
            {
                PublicKey = rosterSizeQuestion1Id,
                StataExportCaption = "var1",
                IsInteger = true
            });
            questionnaire.Children.Add(new NumericQuestion("roster size question 2")
            {
                PublicKey = rosterSizeQuestion2Id,
                StataExportCaption = "var2",
                IsInteger = true
            });
            questionnaire.Children.Add(new Group()
            {
                PublicKey = rosterGroup1Id,
                IsRoster = true,
                VariableName = "a",
                RosterSizeSource = RosterSizeSourceType.Question,
                RosterSizeQuestionId = rosterSizeQuestion1Id,
                RosterTitleQuestionId = rosterTitleQuestionId
            });
            questionnaire.Children.Add(new Group()
            {
                PublicKey = rosterGroup2Id,
                IsRoster = true,
                VariableName = "b",
                RosterSizeSource = RosterSizeSourceType.Question,
                RosterSizeQuestionId = rosterSizeQuestion2Id,
                RosterTitleQuestionId = rosterTitleQuestionId
            });
            verifier = CreateQuestionnaireVerifier();
        };

        Because of = () =>
            resultErrors = verifier.Verify(questionnaire);

        It should_return_2_errors = () =>
             resultErrors.Count().ShouldEqual(2);

        It should_return_2_errors_with_code__WB0035__ = () =>
            resultErrors.ShouldEachConformTo(error => error.Code == "WB0035");

        It should_return_2_errors_with_1_references = () =>
            resultErrors.ShouldEachConformTo(error => error.References.Count() == 1);

        It should_return_error_reference_with_type_group = () =>
            resultErrors.ShouldEachConformTo(error => error.References.First().Type == QuestionnaireVerificationReferenceType.Group);

        It should_return_error_reference_with_id_of_rosterGroup1Id = () =>
            resultErrors.ElementAt(0).References.First().Id.ShouldEqual(rosterGroup1Id);

        It should_return_error_reference_with_id_of_rosterGroup2Id = () =>
            resultErrors.ElementAt(1).References.First().Id.ShouldEqual(rosterGroup2Id);

        private static IEnumerable<QuestionnaireVerificationError> resultErrors;
        private static QuestionnaireVerifier verifier;
        private static QuestionnaireDocument questionnaire;
        private static Guid rosterGroup1Id = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
        private static Guid rosterGroup2Id = Guid.Parse("BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB");
        private static Guid rosterSizeQuestion1Id = Guid.Parse("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC");
        private static Guid rosterSizeQuestion2Id = Guid.Parse("DDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDD");
        private static Guid rosterTitleQuestionId = Guid.Parse("EEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEE");
    }
}
