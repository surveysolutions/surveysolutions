﻿using System;
using System.Linq;
using Machine.Specifications;
using Main.Core.Entities.SubEntities;
using Main.Core.Events.Questionnaire;
using Ncqrs.Spec;
using WB.Core.BoundedContexts.Designer.Aggregates;

namespace WB.Tests.Unit.BoundedContexts.Designer.QuestionnaireTests
{
    internal class when_cloning_question_by_id : QuestionnaireTestsContext
    {
        Establish context = () =>
        {
            questionnaire = CreateQuestionnaire(responsibleId: responsibleId);
            questionnaire.Apply(new NewGroupAdded { PublicKey = chapterId });

            newQuestionAdded = new NewQuestionAdded
            {
                PublicKey = sourceQuestionId, 
                QuestionText = "text",
                VariableLabel = "varlabel",
                QuestionType = QuestionType.MultyOption,
                GroupPublicKey = chapterId,
                AreAnswersOrdered = true,
                MaxAllowedAnswers = 1,
                Featured = true,
                QuestionScope = QuestionScope.Interviewer,
                ConditionExpression = "Conditional",
                ValidationExpression = "Validation",
                ValidationMessage = "Val message",
                Instructions = "Intructions",
                LinkedToQuestionId = Guid.NewGuid(),
                IsFilteredCombobox = true,
                CascadeFromQuestionId = Guid.NewGuid(),
                YesNoView = false
            };
            questionnaire.Apply(newQuestionAdded);

            eventContext = new EventContext();
        };

        Because of = () => questionnaire.CloneQuestionById(sourceQuestionId, responsibleId, questionId);

        It should_copy_property_values_from_source_question = () =>
            eventContext.ShouldContainEvent<QuestionCloned>(e =>
            {
                return e.QuestionType == QuestionType.MultyOption &&
                       string.IsNullOrEmpty(e.StataExportCaption) &&
                       e.PublicKey == questionId &&
                       e.GroupPublicKey == chapterId &&
                       e.QuestionText == newQuestionAdded.QuestionText &&
                       e.VariableLabel == newQuestionAdded.VariableLabel &&
                       e.Featured == true &&
                       e.QuestionScope == QuestionScope.Interviewer &&
                       e.ConditionExpression == newQuestionAdded.ConditionExpression &&
                       e.ValidationExpression == newQuestionAdded.ValidationExpression &&
                       e.ValidationMessage == newQuestionAdded.ValidationMessage &&
                       e.Instructions == newQuestionAdded.Instructions &&
                       e.SourceQuestionId == sourceQuestionId &&
                       e.TargetIndex == 1 &&
                       e.ResponsibleId == responsibleId &&
                       e.AreAnswersOrdered == newQuestionAdded.AreAnswersOrdered &&
                       e.MaxAllowedAnswers == newQuestionAdded.MaxAllowedAnswers &&
                       e.IsFilteredCombobox == newQuestionAdded.IsFilteredCombobox &&
                       e.SourceQuestionnaireId == questionnaire.EventSourceId &&
                       e.YesNoView == newQuestionAdded.YesNoView;
            });
        
        // If we extend QuestionCloned be sure to add check in the validation above and increase counter here
        It should_copy_all_known_properties = () => typeof(QuestionCloned).GetProperties().Count().ShouldEqual(29);

        static Questionnaire questionnaire;
        static Guid questionId = Guid.Parse("11111111111111111111111111111111");
        static Guid chapterId = Guid.Parse("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC");
        static Guid responsibleId = Guid.Parse("DDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDD");
        static Guid sourceQuestionId = Guid.Parse("44444444444444444444444444444444");
        private static EventContext eventContext;
        private static NewQuestionAdded newQuestionAdded;
    }
}

