using System;
using System.Collections.Generic;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.Invariants;

namespace WB.Core.SharedKernels.DataCollection.Implementation.Aggregates
{
    public partial class Interview
    {
        public void AnswerSingleOptionQuestion(Guid userId, Guid questionId, RosterVector rosterVector, DateTime answerTime, decimal selectedValue)
        {
            new InterviewPropertiesInvariants(this.properties).RequireAnswerCanBeChanged();

            var answeredQuestion = new Identity(questionId, rosterVector);

            IQuestionnaire questionnaire = this.GetQuestionnaireOrThrow(this.questionnaireId, this.questionnaireVersion, this.language);

            var sourceInterviewTree = this.BuildInterviewTree(questionnaire, this.interviewState);

            this.CheckSingleOptionQuestionInvariants(questionId, rosterVector, selectedValue, questionnaire, answeredQuestion, sourceInterviewTree);

            var changedInterviewTree = sourceInterviewTree.Clone();

            var changedQuestionIdentities = new List<Identity> { answeredQuestion };
            var question = changedInterviewTree.GetQuestion(answeredQuestion).AsSingleOption;
            var questionWasAnsweredAndAnswerChanged = question.IsAnswered && question.GetAnswer() != selectedValue;
            question.SetAnswer(Convert.ToInt32(selectedValue));
            
            if (questionWasAnsweredAndAnswerChanged)
            {
                RemoveAnswersForDependendCascadingQuestions(answeredQuestion, changedInterviewTree, questionnaire, changedQuestionIdentities);
            }

            this.ApplyQuestionAnswer(userId, changedInterviewTree, questionnaire, changedQuestionIdentities, sourceInterviewTree);
        }
    }
}