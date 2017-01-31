using System;
using System.Collections.Generic;
using System.Linq;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.InterviewEntities;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.Invariants;

namespace WB.Core.SharedKernels.DataCollection.Implementation.Aggregates
{
    public partial class Interview
    {
        public void RemoveAnswer(Guid questionId, RosterVector rosterVector, Guid userId, DateTime removeTime)
        {
            new InterviewPropertiesInvariants(this.properties)
                .RequireAnswerCanBeChanged();

            var questionIdentity = new Identity(questionId, rosterVector);

            IQuestionnaire questionnaire = this.GetQuestionnaireOrThrow();

            var treeInvariants = new InterviewTreeInvariants(this.Tree);

            new InterviewQuestionInvariants(this.properties.Id, questionId, questionnaire)
                .RequireQuestion();

            treeInvariants.RequireRosterVectorQuestionInstanceExists(questionId, rosterVector);
            treeInvariants.RequireQuestionIsEnabled(questionIdentity);

            var changedInterviewTree = this.Tree.Clone();

            var givenAndRemovedAnswers = new List<Identity> { questionIdentity };
            changedInterviewTree.GetQuestion(questionIdentity).RemoveAnswer();

            RemoveAnswersForDependendCascadingQuestions(questionIdentity, changedInterviewTree, questionnaire, givenAndRemovedAnswers);

            changedInterviewTree.ActualizeTree();

            this.UpdateTreeWithDependentChanges(changedInterviewTree, givenAndRemovedAnswers, questionnaire);
            var treeDifference = FindDifferenceBetweenTrees(this.Tree, changedInterviewTree);

            this.ApplyEvents(treeDifference, userId);
        }

        private static void RemoveAnswersForDependendCascadingQuestions(Identity questionIdentity, InterviewTree changedInterviewTree, IQuestionnaire questionnaire, List<Identity> givenAndRemovedAnswers)
        {
            IEnumerable<Guid> dependentQuestionIds = questionnaire.GetCascadingQuestionsThatDependUponQuestion(questionIdentity.Id);
            foreach (var dependentQuestionId in dependentQuestionIds)
            {
                var cascadingAnsweredQuestionsToRemoveAnswer = changedInterviewTree.FindQuestions(dependentQuestionId)
                    .Where(x => x.IsCascading && x.IsAnswered())
                    .Where(x => x.IsOnTheSameOrDeeperLevel(questionIdentity));

                foreach (var cascadingQuestion in cascadingAnsweredQuestionsToRemoveAnswer)
                {
                    cascadingQuestion.RemoveAnswer();
                    givenAndRemovedAnswers.Add(cascadingQuestion.Identity);
                }
            }
        }
    }
}