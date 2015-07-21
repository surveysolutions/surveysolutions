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
    internal class when_verifying_questionnaire_with_question_that_has_substitutions_references_with_deeper_roster_level : QuestionnaireVerifierTestsContext
    {
        Establish context = () =>
        {
            questionWithSubstitutionsId = Guid.Parse("10000000000000000000000000000000");
            underDeeperRosterLevelQuestionId = Guid.Parse("12222222222222222222222222222222");
            var rosterGroupId = Guid.Parse("13333333333333333333333333333333");
            var rosterSizeQuestionId = Guid.Parse("11133333333333333333333333333333");
            questionnaire = CreateQuestionnaireDocument();

            questionnaire.Children.Add(new NumericQuestion()
            {
                PublicKey = rosterSizeQuestionId,
                IsInteger = true,
                StataExportCaption = "var1"
            });
            var rosterGroup = new Group() { PublicKey = rosterGroupId, IsRoster = true, VariableName = "a", RosterSizeQuestionId = rosterSizeQuestionId };

            rosterGroup.Children.Add(new NumericQuestion()
            {
                PublicKey = underDeeperRosterLevelQuestionId,
                StataExportCaption = underDeeperRosterLevelQuestionVariableName
            });
            questionnaire.Children.Add(rosterGroup);
            questionnaire.Children.Add(new SingleQuestion()
            {
                PublicKey = questionWithSubstitutionsId,
                StataExportCaption = "var2",
                QuestionText = string.Format("hello %{0}%", underDeeperRosterLevelQuestionVariableName),
                Answers = { new Answer() { AnswerValue = "1", AnswerText = "opt 1" }, new Answer() { AnswerValue = "2", AnswerText = "opt 2" } }
            });

            verifier = CreateQuestionnaireVerifier();
        };

        Because of = () =>
            resultErrors = verifier.Verify(questionnaire);

        It should_return_1_error = () =>
            resultErrors.Count().ShouldEqual(1);

        It should_return_error_with_code__WB0019 = () =>
            resultErrors.Single().Code.ShouldEqual("WB0019");

        It should_return_error_with_two_references = () =>
            resultErrors.Single().References.Count().ShouldEqual(2);

        It should_return_first_error_reference_with_type_Question = () =>
            resultErrors.Single().References.First().Type.ShouldEqual(QuestionnaireVerificationReferenceType.Question);

        It should_return_first_error_reference_with_id_of_underDeeperPropagationLevelQuestionId = () =>
            resultErrors.Single().References.First().Id.ShouldEqual(questionWithSubstitutionsId);

        It should_return_last_error_reference_with_type_Question = () =>
            resultErrors.Single().References.Last().Type.ShouldEqual(QuestionnaireVerificationReferenceType.Question);

        It should_return_last_error_reference_with_id_of_underDeeperPropagationLevelQuestionVariableName = () =>
            resultErrors.Single().References.Last().Id.ShouldEqual(underDeeperRosterLevelQuestionId);

        private static IEnumerable<QuestionnaireVerificationError> resultErrors;
        private static QuestionnaireVerifier verifier;
        private static QuestionnaireDocument questionnaire;

        private static Guid questionWithSubstitutionsId;
        private static Guid underDeeperRosterLevelQuestionId;
        private const string underDeeperRosterLevelQuestionVariableName = "i_am_deeper_ddddd_deeper";
    }
}
