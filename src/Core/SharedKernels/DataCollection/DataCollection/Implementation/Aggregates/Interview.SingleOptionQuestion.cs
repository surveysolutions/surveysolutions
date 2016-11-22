using System;
using System.Collections.Generic;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.InterviewEntities.Answers;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.Invariants;

namespace WB.Core.SharedKernels.DataCollection.Implementation.Aggregates
{
    public partial class Interview
    {
        public void AnswerSingleOptionQuestion(Guid userId, Guid questionId, RosterVector rosterVector, DateTime answerTime, decimal selectedValue)
        {
            new InterviewPropertiesInvariants(this.properties).RequireAnswerCanBeChanged();

            var answeredQuestion = new Identity(questionId, rosterVector);

            IQuestionnaire questionnaire = this.GetQuestionnaireOrThrow();

            var sourceInterviewTree = this.Tree;
            var isLinkedToList = sourceInterviewTree.GetQuestion(answeredQuestion).IsLinkedToListQuestion;
            this.CheckSingleOptionQuestionInvariants(questionId, rosterVector, selectedValue, questionnaire, answeredQuestion, sourceInterviewTree, isLinkedToList);

            var changedInterviewTree = sourceInterviewTree.Clone();

            var changedQuestionIdentities = new List<Identity> { answeredQuestion };
            var singleQuestion = changedInterviewTree.GetQuestion(answeredQuestion);
            
            if (isLinkedToList)
            {
                var question = singleQuestion.AsSingleLinkedToList;
                question.SetAnswer(CategoricalFixedSingleOptionAnswer.FromInt(Convert.ToInt32(selectedValue)));
            }
            else
            {
                var question = singleQuestion.AsSingleFixedOption;
                var questionWasAnsweredAndAnswerChanged = question.IsAnswered && question.GetAnswer().SelectedValue != selectedValue;
                question.SetAnswer(CategoricalFixedSingleOptionAnswer.FromInt(Convert.ToInt32(selectedValue)));

                if (questionWasAnsweredAndAnswerChanged)
                {
                    RemoveAnswersForDependendCascadingQuestions(answeredQuestion, changedInterviewTree, questionnaire, changedQuestionIdentities);
                }
            }

            this.ApplyTreeDiffChanges(userId, changedInterviewTree, questionnaire, changedQuestionIdentities, sourceInterviewTree);
        }
    }
}