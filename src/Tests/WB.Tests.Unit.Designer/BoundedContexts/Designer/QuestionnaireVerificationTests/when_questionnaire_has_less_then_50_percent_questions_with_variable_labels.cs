using System;
using System.Collections.Generic;
using System.Linq;
using Machine.Specifications;
using Main.Core.Documents;
using Main.Core.Entities.SubEntities;
using Moq;
using WB.Core.BoundedContexts.Designer.Implementation.Services;
using WB.Core.BoundedContexts.Designer.Services;
using WB.Core.BoundedContexts.Designer.ValueObjects;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.Designer.BoundedContexts.Designer.QuestionnaireVerificationTests
{
    internal class when_questionnaire_has_less_then_50_percent_questions_with_variable_labels : QuestionnaireVerifierTestsContext
    {
        Establish context = () =>
        {
            
            questionnaire = CreateQuestionnaireDocumentWithOneChapter(
                Create.GpsCoordinateQuestion(questionId: questionId1, variable:"q1"),
                Create.Question(questionId: questionId2, questionType: QuestionType.DateTime,
                        variable: "q2", variableLabel:"label"),
                Create.Question(questionId: questionId3, questionType: QuestionType.Text, variable: "q3"),
                Create.TextQuestion(questionId: prefilledQuestionId, variable: "q4", scope: QuestionScope.Headquarter));
            

            verifier = CreateQuestionnaireVerifier();
        };

        Because of = () => errors = verifier.Verify(questionnaire);

        It should_return_WB0253_warning = () => errors.ShouldContainWarning("WB0253", "Too few variable labels are defined. Add variable labels to improve the usability of exported data and to provide input into metadata for Data Documentation Initiative (DDI) format.");

        It should_not_return_WB0253_warning_for_prefilled_question = () => errors.GetWarnings("WB0253").Count().ShouldEqual(1);

        static QuestionnaireDocument questionnaire;
        static QuestionnaireVerifier verifier;
        static IEnumerable<QuestionnaireVerificationMessage> errors;
        static Guid questionId1 = Guid.Parse("DDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDD");
        static Guid questionId2 = Guid.Parse("1111DDDDDDDDDDDDDDDDDDDDDDDDDDDD");
        static Guid questionId3 = Guid.Parse("2222DDDDDDDDDDDDDDDDDDDDDDDDDDDD");
        static Guid prefilledQuestionId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
    }
}