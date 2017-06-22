using System;
using System.Collections.Generic;
using Machine.Specifications;
using Main.Core.Entities.SubEntities;
using WB.Core.BoundedContexts.Designer.Aggregates;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Base;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Question;
using WB.Core.BoundedContexts.Designer.Exceptions;
using WB.Core.SharedKernels.QuestionnaireEntities;

namespace WB.Tests.Unit.Designer.BoundedContexts.QuestionnaireTests
{
    internal class when_updating_question_and_error_message_contains_substitution_to_question_from_roster : QuestionnaireTestsContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            questionnaire = CreateQuestionnaire(responsibleId: responsibleId);
            questionnaire.AddGroup(chapterId, responsibleId:responsibleId);
            questionnaire.AddGroup(rosterId, responsibleId: responsibleId, isRoster:true);
            
            questionnaire.AddTextQuestion(textQuestionId, rosterId, responsibleId, variableName: textQuestionVariable);
            
            questionnaire.AddTextQuestion(questionWithSubstitutionId, chapterId, responsibleId);


            BecauseOf();
        }

        private void BecauseOf() => exception =
            Catch.Exception(() =>
            questionnaire.UpdateTextQuestion(
                new UpdateTextQuestion(
                    questionnaire.Id,
                    questionWithSubstitutionId,
                    responsibleId,
                    new CommonQuestionParameters() { Title = "title1", VariableName = "q1" },
                    isPreFilled: false,
                    validationConditions: new List<ValidationCondition>() {new ValidationCondition
                        {
                            Message = $"error message with substitution %{textQuestionVariable}%"
                        } },

                    scope: QuestionScope.Interviewer,
                    mask: null)));

        [NUnit.Framework.Test] public void should_exception_has_specified_message () =>
            new[] { "illegal", "substitution", "to", textQuestionVariable }.ShouldEachConformTo(x =>
                ((QuestionnaireException) exception).Message.Contains(x));

        private static Questionnaire questionnaire;
        private static Guid responsibleId = Guid.Parse("DDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDD");
        private static Guid chapterId = Guid.Parse("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC");
        private static Guid questionWithSubstitutionId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
        private static Guid rosterId = Guid.Parse("EEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEE");
        private static Guid textQuestionId = Guid.Parse("22222222222222222222222222222222");
        private static string textQuestionVariable = "hhname";
        private static Exception exception;
    }
}