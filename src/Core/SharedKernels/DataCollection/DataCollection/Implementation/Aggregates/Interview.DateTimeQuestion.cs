using System;
using System.Collections.Generic;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.Invariants;

namespace WB.Core.SharedKernels.DataCollection.Implementation.Aggregates
{
    public partial class Interview
    {
        public void AnswerDateTimeQuestion(Guid userId, Guid questionId, RosterVector rosterVector, DateTime answerTime, DateTime answer)
        {
            new InterviewPropertiesInvariants(this.properties).RequireAnswerCanBeChanged();

            var answeredQuestion = new Identity(questionId, rosterVector);

            IQuestionnaire questionnaire = this.GetQuestionnaireOrThrow(this.questionnaireId, this.questionnaireVersion, this.language);
            this.CheckDateTimeQuestionInvariants(questionId, rosterVector, questionnaire, answeredQuestion, this.interviewState);

            var sourceInterviewTree = this.BuildInterviewTree(questionnaire, this.interviewState);
            var changedInterviewTree = sourceInterviewTree.Clone();
            var changedQuestionIdentities = new List<Identity> { answeredQuestion };
            changedInterviewTree.GetQuestion(answeredQuestion).AsDateTime.SetAnswer(answer);

            this.ApplyQuestionAnswer(userId, changedInterviewTree, questionnaire, changedQuestionIdentities, sourceInterviewTree);
        }
    }
}