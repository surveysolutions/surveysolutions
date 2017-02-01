using System;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Commands.Interview;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.InterviewEntities;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.InterviewEntities.Answers;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.Invariants;

namespace WB.Core.SharedKernels.DataCollection.Implementation.Aggregates
{
    public partial class Interview
    {
        public void AnswerNumericIntegerQuestion(AnswerNumericIntegerQuestionCommand command)
            => AnswerNumericIntegerQuestion(command.UserId, command.QuestionId, command.RosterVector, command.AnswerTime, command.Answer);

        internal void AnswerNumericIntegerQuestion(Guid userId, Guid questionId, RosterVector rosterVector, DateTime answerTime, int answer)
        {
            new InterviewPropertiesInvariants(this.properties)
                .RequireAnswerCanBeChanged();

            var questionIdentity = new Identity(questionId, rosterVector);

            IQuestionnaire questionnaire = this.GetQuestionnaireOrThrow();

            RequireNumericIntegerAnswerAllowed(questionIdentity, answer, questionnaire, this.Tree);

            var changedInterviewTree = this.Tree.Clone();

            changedInterviewTree.GetQuestion(questionIdentity).AsInteger.SetAnswer(NumericIntegerAnswer.FromInt(answer));

            changedInterviewTree.ActualizeTree();

            this.UpdateTreeWithDependentChanges(changedInterviewTree, new [] { questionIdentity }, questionnaire);
            var treeDifference = FindDifferenceBetweenTrees(this.Tree, changedInterviewTree);

            this.ApplyEvents(treeDifference, userId);
        }
    }
}
