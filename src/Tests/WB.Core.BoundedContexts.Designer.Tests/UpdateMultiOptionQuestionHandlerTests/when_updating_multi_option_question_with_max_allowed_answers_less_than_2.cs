using System;
using Machine.Specifications;
using Main.Core.Entities.SubEntities;
using Main.Core.Events.Questionnaire;
using WB.Core.BoundedContexts.Designer.Aggregates;
using WB.Core.BoundedContexts.Designer.Events.Questionnaire;
using WB.Core.BoundedContexts.Designer.Exceptions;
using WB.Core.BoundedContexts.Designer.Tests.QuestionnaireTests;

namespace WB.Core.BoundedContexts.Designer.Tests.UpdateMultiOptionQuestionHandlerTests
{
    internal class when_updating_multi_option_question_with_max_allowed_answers_less_than_2 : QuestionnaireTestsContext
    {
        Establish context = () =>
        {
            questionnaire = CreateQuestionnaire(responsibleId: responsibleId);
            questionnaire.Apply(new NewGroupAdded { PublicKey = parentGroupId });
            questionnaire.Apply(new QRBarcodeQuestionAdded
            {
                QuestionId = questionId,
                ParentGroupId = parentGroupId,
                Title = "old title",
                VariableName = "old_variable_name",
                IsMandatory = false,
                Instructions = "old instructions",
                EnablementCondition = "old condition",
                ResponsibleId = responsibleId
            });
        };

        Because of = () =>
            exception = Catch.Exception(() =>
                questionnaire.UpdateMultiOptionQuestion(
                    questionId: questionId,
                    title: "title",
                    variableName: "var",
                    isMandatory: false,
                    scope: QuestionScope.Interviewer,
                    enablementCondition: null,
                    validationExpression: null,
                    validationMessage: null,
                    instructions: null,
                    responsibleId: responsibleId,
                    options: new Option[]
                    {
                        new Option(Guid.NewGuid(), "1", "opt1Title"),
                        new Option(Guid.NewGuid(), "2", "opt2Title")
                    },
                    linkedToQuestionId: null,
                    areAnswersOrdered: false,
                    maxAllowedAnswers: 1
                    ));


        It should_throw_QuestionnaireException = () =>
            exception.ShouldBeOfExactType<QuestionnaireException>();

        It should_throw_exception_with_message_containting__maximum_allowed_answers_should_be_more_than_one__ = () =>
            new[] { "maximum allowed answers for question should be more than one" }.ShouldEachConformTo(
                keyword => exception.Message.ToLower().Contains(keyword));

        private static Exception exception;
        private static Questionnaire questionnaire;
        private static Guid questionId = Guid.Parse("11111111111111111111111111111111");
        private static Guid responsibleId = Guid.Parse("DDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDD");
        private static Guid parentGroupId = Guid.Parse("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC");
    }
}