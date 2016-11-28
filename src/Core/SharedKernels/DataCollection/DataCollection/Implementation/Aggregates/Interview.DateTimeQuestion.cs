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
        public void AnswerDateTimeQuestion(Guid userId, Guid questionId, RosterVector rosterVector, DateTime answerTime, DateTime answer)
        {
            new InterviewPropertiesInvariants(this.properties).RequireAnswerCanBeChanged();

            var answeredQuestion = new Identity(questionId, rosterVector);

            IQuestionnaire questionnaire = this.GetQuestionnaireOrThrow();

            this.CheckDateTimeQuestionInvariants(questionId, rosterVector, questionnaire, answeredQuestion, this.Tree);
            
            var changedInterviewTree = this.Tree.Clone();
            changedInterviewTree.GetQuestion(answeredQuestion).AsDateTime.SetAnswer(DateTimeAnswer.FromDateTime(answer));

            this.UpdateTreeWithDependentChanges(changedInterviewTree, new [] { answeredQuestion }, questionnaire);

            this.ApplyEvents(changedInterviewTree, userId);
        }
    }
}