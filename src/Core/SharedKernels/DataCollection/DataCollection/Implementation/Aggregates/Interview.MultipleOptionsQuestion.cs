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
            new InterviewPropertiesInvariants(this.properties)
                .RequireAnswerCanBeChanged();

            var questionIdentity = new Identity(questionId, rosterVector);

            IQuestionnaire questionnaire = this.GetQuestionnaireOrThrow();

            var isLinkedToList = this.Tree.GetQuestion(questionIdentity).IsLinkedToListQuestion;

            if (isLinkedToList)
                RequireLinkedToListMultipleOptionsAnswerAllowed(questionIdentity, selectedValues, questionnaire, this.Tree);
            else
                RequireFixedMultipleOptionsAnswerAllowed(questionIdentity, selectedValues, questionnaire, this.Tree);

            var changedInterviewTree = this.Tree.Clone();

            if (isLinkedToList)
            {
                changedInterviewTree.GetQuestion(questionIdentity).AsMultiLinkedToList.SetAnswer(CategoricalFixedMultiOptionAnswer.FromInts(selectedValues));
            }
            else
                changedInterviewTree.GetQuestion(questionIdentity).AsMultiFixedOption.SetAnswer(CategoricalFixedMultiOptionAnswer.FromInts(selectedValues));

            changedInterviewTree.ActualizeTree();

            this.UpdateTreeWithDependentChanges(changedInterviewTree, new [] { questionIdentity }, questionnaire);
            var treeDifference = FindDifferenceBetweenTrees(this.Tree, changedInterviewTree);

            this.ApplyEvents(treeDifference, userId);
        }
    }
}