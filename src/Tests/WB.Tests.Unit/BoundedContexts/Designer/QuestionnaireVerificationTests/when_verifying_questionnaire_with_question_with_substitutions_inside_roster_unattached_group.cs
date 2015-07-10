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
    internal class when_verifying_questionnaire_with_question_with_substitutions_inside_roster_unattached_group : QuestionnaireVerifierTestsContext
    {
        Establish context = () =>
        {
            questionWithSubstitutionsId = Guid.Parse("10000000000000000000000000000000");
            underDeeperRosterLevelQuestionId = Guid.Parse("12222222222222222222222222222222");
            var rosterGroupId = Guid.Parse("13333333333333333333333333333333");
            questionnaire = CreateQuestionnaireDocument();
            rosterSizeQuestionId = Guid.Parse("11133333333333333333333333333333");

            questionnaire.Children.Add(new NumericQuestion()
            {
                PublicKey = rosterSizeQuestionId,
                IsInteger = true,
                StataExportCaption="rosterSize"
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

        private It should_return_WB0019_error_with_2_references_on_questions = () =>
            resultErrors.Single()
                .References.ToList()
                .ForEach(question => question.Type.ShouldEqual(QuestionnaireVerificationReferenceType.Question));

        It should_return_WB0019_error_with_first_reference_to_question_with_substitution_text = () =>
            resultErrors.Single().References.ElementAt(0).Id.ShouldEqual(questionWithSubstitutionsId);

        It should_return_WB0019_error_with_second_reference_to_question_that_used_as_substitution_question = () =>
            resultErrors.Single().References.ElementAt(1).Id.ShouldEqual(underDeeperRosterLevelQuestionId);

        private static IEnumerable<QuestionnaireVerificationError> resultErrors;
        private static QuestionnaireVerifier verifier;
        private static QuestionnaireDocument questionnaire;

        private static Guid questionWithSubstitutionsId;
        private static Guid underDeeperRosterLevelQuestionId;
        private static Guid rosterSizeQuestionId;
        private const string underDeeperRosterLevelQuestionVariableName = "i_am_deeper_ddddd_deeper";
    }
}
