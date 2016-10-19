using System;
using System.Collections.Generic;
using WB.Core.SharedKernels.DataCollection.Aggregates;
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

            IQuestionnaire questionnaire = this.GetQuestionnaireOrThrow(this.questionnaireId, this.questionnaireVersion, this.language);

            var sourceInterviewTree = this.BuildInterviewTree(questionnaire, this.interviewState);

            CheckTextListInvariants(questionId, rosterVector, questionnaire, answeredQuestion, this.interviewState, answers);

            var changedInterviewTree = this.BuildInterviewTree(questionnaire, this.interviewState);

            var changedQuestionIdentities = new List<Identity> { answeredQuestion };
            changedInterviewTree.GetQuestion(answeredQuestion).AsTextList.SetAnswer(answers);

            this.ApplyQuestionAnswer(userId, changedInterviewTree, questionnaire, changedQuestionIdentities, sourceInterviewTree);
        }
    }
}