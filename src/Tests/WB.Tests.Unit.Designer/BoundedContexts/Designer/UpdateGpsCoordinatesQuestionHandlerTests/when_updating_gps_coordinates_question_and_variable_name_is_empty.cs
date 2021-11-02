using System;
using System.Collections.Generic;
using Main.Core.Entities.SubEntities;
using WB.Core.BoundedContexts.Designer.Aggregates;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Base;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Question;

using WB.Core.SharedKernels.QuestionnaireEntities;
using WB.Tests.Unit.Designer.BoundedContexts.QuestionnaireTests;

namespace WB.Tests.Unit.Designer.BoundedContexts.Designer.UpdateGpsCoordinatesQuestionHandlerTests
{
    internal class when_updating_gps_coordinates_question_and_variable_name_is_empty : QuestionnaireTestsContext
    {
        [NUnit.Framework.Test]
        public void should_not_throw_exception()
        {
            questionnaire = CreateQuestionnaire(responsibleId: responsibleId);
            questionnaire.AddGroup(chapterId, responsibleId: responsibleId);
            questionnaire.AddQRBarcodeQuestion(
                questionId,
                chapterId,
                title: "old title",
                variableName: "old_variable_name",
                instructions: "old instructions",
                enablementCondition: "old condition",
                responsibleId: responsibleId);

            questionnaire.UpdateGpsCoordinatesQuestion(
                new UpdateGpsCoordinatesQuestion(
                    questionnaire.Id,
                    questionId: questionId,
                    commonQuestionParameters: new CommonQuestionParameters()
                    {
                        Title = title,
                        VariableName = variableName,
                        Instructions = instructions,
                        EnablementCondition = enablementCondition
                    },
                    validationMessage: null,
                    validationExpression: null,
                    isPreFilled: false,
                    scope: scope,
                    responsibleId: responsibleId,
                    validationConditions: new List<ValidationCondition>()));
        }

        private static Questionnaire questionnaire;
        private static Guid questionId = Guid.Parse("11111111111111111111111111111111");
        private static Guid chapterId = Guid.Parse("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC");
        private static Guid responsibleId = Guid.Parse("DDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDD");
        private static string variableName = "";
        private static string title = "title";
        private static string instructions = "intructions";
        private static QuestionScope scope = QuestionScope.Interviewer;
        private static string enablementCondition = null;
    }
}
