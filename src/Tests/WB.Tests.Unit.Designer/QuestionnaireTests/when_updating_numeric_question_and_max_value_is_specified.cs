using System;
using System.Collections.Generic;
using Machine.Specifications;
using Main.Core.Entities.SubEntities;
using WB.Core.BoundedContexts.Designer.Aggregates;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Base;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Question;

using WB.Core.SharedKernels.QuestionnaireEntities;

namespace WB.Tests.Unit.Designer.BoundedContexts.QuestionnaireTests
{
    internal class when_updating_numeric_question_and_max_value_is_specified : QuestionnaireTestsContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            responsibleId = Guid.Parse("DDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDD");
            chapterId = Guid.Parse("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC");
            questionId = Guid.Parse("11111111111111111111111111111111");

            questionnaire = CreateQuestionnaire(responsibleId: responsibleId);
            questionnaire.AddGroup(chapterId, responsibleId:responsibleId);
            questionnaire.AddTextQuestion(questionId, chapterId, responsibleId);
            BecauseOf();
        }

        private void BecauseOf() =>

        questionnaire.UpdateNumericQuestion(
                new UpdateNumericQuestion(
                    questionnaire.Id,
                    questionId,
                    responsibleId,
                    new CommonQuestionParameters() { Title = "title", VariableName = "var1" },
                    false,
                    QuestionScope.Interviewer,
                    useFormatting: false,
                    isInteger: false,
                    countOfDecimalPlaces: null,
                    validationConditions: new List<ValidationCondition>(),
                    options: null));


        [NUnit.Framework.Test] public void should_contains_question () =>
            questionnaire.QuestionnaireDocument.Find<IQuestion>(questionId).ShouldNotBeNull();

        [NUnit.Framework.Test] public void should_contains_question_with_PublicKey_equal_to_question_id () =>
            questionnaire.QuestionnaireDocument.Find<IQuestion>(questionId)
                .PublicKey.ShouldEqual(questionId);

        private static Questionnaire questionnaire;
        private static Guid questionId;
        private static Guid chapterId;
        private static Guid responsibleId;
    }
}