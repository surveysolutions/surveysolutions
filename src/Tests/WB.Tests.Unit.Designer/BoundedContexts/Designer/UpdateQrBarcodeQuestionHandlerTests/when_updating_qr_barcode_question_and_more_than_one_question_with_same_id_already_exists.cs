using System;
using System.Collections.Generic;
using Machine.Specifications;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using WB.Core.BoundedContexts.Designer.Aggregates;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Base;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Question;
using WB.Core.BoundedContexts.Designer.Exceptions;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.QuestionnaireDto;
using WB.Core.SharedKernels.QuestionnaireEntities;
using WB.Tests.Unit.Designer.BoundedContexts.QuestionnaireTests;

namespace WB.Tests.Unit.Designer.BoundedContexts.Designer.UpdateQrBarcodeQuestionHandlerTests
{
    internal class when_updating_qr_barcode_question_and_more_than_one_question_with_same_id_already_exists : QuestionnaireTestsContext
    {
        Establish context = () =>
        {
            questionId = Guid.Parse("BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB");

            var answers = new List<Answer>() { new Answer() { AnswerCode = 1, AnswerText = "1" }, new Answer() { AnswerCode = 2, AnswerText = "2" } };

            var questionnaireDoc = Create.QuestionnaireDocument(
                children: new List<IComposite>
                {
                    Create.SingleQuestion(id:questionId,isFilteredCombobox:false, options:answers),
                    Create.SingleQuestion(id:questionId,isFilteredCombobox:false, options:answers)
                });

            questionnaire = Create.Questionnaire();
            questionnaire.Initialize(Guid.NewGuid(), questionnaireDoc, null);
            
        };

        Because of = () =>
            exception = Catch.Exception(() =>
                questionnaire.UpdateQRBarcodeQuestion(
                    new UpdateQRBarcodeQuestion(
                        questionnaire.Id,
                        questionId: questionId,
                        commonQuestionParameters: new CommonQuestionParameters()
                        {
                            Title = "title",
                            VariableName = "var",

                        },
                        validationExpression: null,
                        validationMessage: null,
                        responsibleId: responsibleId,
                        scope: QuestionScope.Interviewer,
                        validationConditions: new System.Collections.Generic.List<WB.Core.SharedKernels.QuestionnaireEntities.ValidationCondition>())));

        It should_throw_QuestionnaireException = () =>
            exception.ShouldBeOfExactType<QuestionnaireException>();

        It should_throw_exception_with_message_containting__more__question__exist__ = () =>
             new[] { "more", "question", "exist" }.ShouldEachConformTo(
                    keyword => exception.Message.ToLower().Contains(keyword));

        
        private static Questionnaire questionnaire;
        private static Exception exception;
        private static Guid questionId = Guid.Parse("11111111111111111111111111111111");
        private static Guid chapterId = Guid.Parse("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC");
        private static Guid responsibleId = Guid.Parse("DDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDD");
    }
}