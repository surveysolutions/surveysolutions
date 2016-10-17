using System;
using System.Collections.Generic;
using System.Linq;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Events.Interview.Dtos;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.InterviewEntities;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.Invariants;

namespace WB.Core.SharedKernels.DataCollection.Implementation.Aggregates
{
    public partial class Interview
    {
        public void AnswerSingleOptionLinkedQuestion(Guid userId, Guid questionId, RosterVector rosterVector, DateTime answerTime, decimal[] selectedRosterVector)
        {
            new InterviewPropertiesInvariants(this.properties).RequireAnswerCanBeChanged();

            var answeredQuestion = new Identity(questionId, rosterVector);

            IQuestionnaire questionnaire = this.GetQuestionnaireOrThrow(this.questionnaireId, this.questionnaireVersion, this.language);

            this.CheckLinkedSingleOptionQuestionInvariants(questionId, rosterVector, selectedRosterVector, questionnaire, answeredQuestion);

            ILatestInterviewExpressionState expressionProcessorState = this.GetClonedExpressionState();

            expressionProcessorState.UpdateLinkedSingleOptionAnswer(questionId, rosterVector, selectedRosterVector);

            InterviewChanges interviewChanges = this.CalculateInterviewChangesOnAnswerQuestion(userId, questionId, rosterVector, selectedRosterVector,
                AnswerChangeType.SingleOptionLinked, answerTime, questionnaire, expressionProcessorState);

            this.ApplyInterviewChanges(interviewChanges);
        }
    }
}