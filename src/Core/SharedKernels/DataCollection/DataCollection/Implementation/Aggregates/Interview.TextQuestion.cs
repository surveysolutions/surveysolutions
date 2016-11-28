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
        public void AnswerTextQuestion(Guid userId, Guid questionId, RosterVector rosterVector, DateTime answerTime, string answer)
        {
            new InterviewPropertiesInvariants(this.properties).RequireAnswerCanBeChanged();

            var answeredQuestion = new Identity(questionId, rosterVector);

            IQuestionnaire questionnaire = this.GetQuestionnaireOrThrow();

            this.CheckTextQuestionInvariants(questionId, rosterVector, questionnaire, answeredQuestion, this.Tree);

            var changedInterviewTree = this.Tree.Clone();

            changedInterviewTree.GetQuestion(answeredQuestion).AsText.SetAnswer(TextAnswer.FromString(answer));

            this.UpdateTreeWithDependentChanges(changedInterviewTree, new [] { answeredQuestion }, questionnaire);

            this.ApplyEvents(changedInterviewTree, userId);
        }
    }
}