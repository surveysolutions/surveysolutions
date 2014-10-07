using System;
using System.Linq;
using Machine.Specifications;
using Main.Core.Entities.SubEntities;
using Main.Core.Events.Questionnaire;
using Ncqrs.Spec;
using WB.Core.BoundedContexts.Designer.Aggregates;
using WB.Core.BoundedContexts.Designer.Tests.QuestionnaireTests;

namespace WB.Core.BoundedContexts.Designer.Tests.UpdateSingleOptionQuestionHandlerTests
{
    internal class when_updating_cascading_combobox_question_that_was_filtered_combobox_and_options_are_more_then_200 : QuestionnaireTestsContext
    {
        Establish context = () =>
        {
            int incrementer = 0;
            oldAnswers = new Answer[210].Select(
                answer =>
                    new Answer
                    {
                        AnswerValue = incrementer.ToString(),
                        AnswerText = (incrementer++).ToString()
                    }).ToArray();

            questionnaire = CreateQuestionnaire(responsibleId: responsibleId);
            questionnaire.Apply(new NewGroupAdded { PublicKey = chapterId });
            questionnaire.Apply(new NewQuestionAdded
            {
                PublicKey = parentQuestionId,
                Answers = new Answer[] { new Answer { AnswerText = "option1", AnswerValue = "1" }, new Answer { AnswerText = "option2", AnswerValue = "2" } },
                GroupPublicKey = chapterId,
                QuestionText = "Parent question",
                QuestionType = QuestionType.SingleOption,
                StataExportCaption = "cascade_parent",
                Featured = false,
                ResponsibleId = responsibleId,
                LinkedToQuestionId = null,
                IsFilteredCombobox = false,
                CascadeFromQuestionId = null
            });

            questionnaire.Apply(new NewQuestionAdded
            {
                PublicKey = filteredQuestionId,
                Answers = oldAnswers,
                GroupPublicKey = chapterId,
                QuestionText = "Filtered combobox question",
                QuestionType = QuestionType.SingleOption,
                StataExportCaption = "filtered",
                Featured = false,
                ResponsibleId = responsibleId,
                LinkedToQuestionId = null,
                IsFilteredCombobox = true,
                CascadeFromQuestionId = null
            });

            eventContext = new EventContext();
        };

        Because of = () =>
            questionnaire.UpdateSingleOptionQuestion(
                questionId: filteredQuestionId,
                title: "title",
                variableName: "qr_barcode_question",
                variableLabel: null,
                isMandatory: true,
                isPreFilled: false,
                scope: QuestionScope.Interviewer,
                enablementCondition: "some condition",
                validationExpression: "some validation",
                validationMessage: "validation message",
                instructions: "intructions",
                responsibleId: responsibleId,
                options: null,
                linkedToQuestionId: (Guid?)null,
                isFilteredCombobox: false,
                cascadeFromQuestionId: parentQuestionId);

        private Cleanup stuff = () =>
        {
            eventContext.Dispose();
            eventContext = null;
        };

        It should_raise_QuestionChanged_event = () =>
            eventContext.ShouldContainEvent<QuestionChanged>();
        
        It should_raise_QuestionChanged_event_with_answer_option_that_was_presiously_saved = () =>
            eventContext.GetSingleEvent<QuestionChanged>()
                .Answers.Count().ShouldEqual(oldAnswers.Count());

        private static EventContext eventContext;
        private static Questionnaire questionnaire;
        private static Guid filteredQuestionId = Guid.Parse("11111111111111111111111111111111");
        private static Guid parentQuestionId = Guid.Parse("22222222222222222222222222222222");
        private static Guid chapterId = Guid.Parse("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC");
        private static Guid responsibleId = Guid.Parse("DDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDD");
        private static Answer[] oldAnswers;
    }
}