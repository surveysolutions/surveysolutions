using System;
using System.Collections.Generic;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.Invariants;

namespace WB.Core.SharedKernels.DataCollection.Implementation.Aggregates
{
    public partial class Interview
    {
        public void AnswerNumericRealQuestion(Guid userId, Guid questionId, RosterVector rosterVector, DateTime answerTime, decimal answer)
        {
            new InterviewPropertiesInvariants(this.properties).RequireAnswerCanBeChanged();

            var answeredQuestion = new Identity(questionId, rosterVector);
            IQuestionnaire questionnaire = this.GetQuestionnaireOrThrow(this.questionnaireId, this.questionnaireVersion, this.language);
            this.CheckNumericRealQuestionInvariants(questionId, rosterVector, answer, questionnaire, answeredQuestion, this.interviewState);

            var sourceInterviewTree = this.BuildInterviewTree(questionnaire, this.interviewState);
            var changedInterviewTree = this.BuildInterviewTree(questionnaire, this.interviewState);
            var changedQuestionIdentities = new List<Identity> { answeredQuestion };
            changedInterviewTree.GetQuestion(answeredQuestion).AsDouble.SetAnswer((double)answer);

            this.ApplyQuestionAnswer(userId, changedInterviewTree, questionnaire, changedQuestionIdentities, sourceInterviewTree);
        }
    }
}