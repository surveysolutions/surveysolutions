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
        public void AnswerTextListQuestion(Guid userId, Guid questionId, RosterVector rosterVector, DateTime answerTime,
            Tuple<decimal, string>[] answers)
        {
            new InterviewPropertiesInvariants(this.properties).RequireAnswerCanBeChanged();

            var answeredQuestion = new Identity(questionId, rosterVector);

            IQuestionnaire questionnaire = this.GetQuestionnaireOrThrow();

            this.CheckTextListInvariants(questionId, rosterVector, questionnaire, answeredQuestion, answers, this.Tree);

            var changedInterviewTree = this.Tree.Clone();

            changedInterviewTree.GetQuestion(answeredQuestion).AsTextList.SetAnswer(TextListAnswer.FromTupleArray(answers));

            changedInterviewTree.ActualizeTree();

            this.UpdateTreeWithDependentChanges(changedInterviewTree, new [] { answeredQuestion }, questionnaire);

            this.ApplyEvents(changedInterviewTree, userId);
        }
    }
}