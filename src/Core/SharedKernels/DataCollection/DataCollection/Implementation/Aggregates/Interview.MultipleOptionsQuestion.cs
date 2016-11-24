using System;
using System.Collections.Generic;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.InterviewEntities;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.InterviewEntities.Answers;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.Invariants;

namespace WB.Core.SharedKernels.DataCollection.Implementation.Aggregates
{
    public partial class Interview
    {
        public void AnswerMultipleOptionsQuestion(Guid userId, Guid questionId, RosterVector rosterVector, DateTime answerTime, int[] selectedValues)
        {
            new InterviewPropertiesInvariants(this.properties).RequireAnswerCanBeChanged();

            var answeredQuestion = new Identity(questionId, rosterVector);

            IQuestionnaire questionnaire = this.GetQuestionnaireOrThrow();

            var isLinkedToList = this.Tree.GetQuestion(answeredQuestion).IsLinkedToListQuestion;

            this.CheckMultipleOptionQuestionInvariants(questionId, rosterVector, selectedValues, questionnaire, answeredQuestion, this.Tree, isLinkedToList);

            var changedInterviewTree = this.Tree.Clone();
            var changedQuestionIdentities = new List<Identity> { answeredQuestion };

            if (isLinkedToList)
            {
                changedInterviewTree.GetQuestion(answeredQuestion).AsMultiLinkedToList.SetAnswer(CategoricalFixedMultiOptionAnswer.FromInts(selectedValues));
            }
            else
                changedInterviewTree.GetQuestion(answeredQuestion).AsMultiFixedOption.SetAnswer(CategoricalFixedMultiOptionAnswer.FromInts(selectedValues));

            changedInterviewTree.ActualizeTree();

            this.CalculateTreeDiffChanges(changedInterviewTree, questionnaire, changedQuestionIdentities);

            this.ApplyEvents(changedInterviewTree, userId);
        }
    }
}