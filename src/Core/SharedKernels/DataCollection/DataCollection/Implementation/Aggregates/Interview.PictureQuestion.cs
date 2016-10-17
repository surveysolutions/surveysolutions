using System;
using System.Collections.Generic;
using System.Linq;
using Main.Core.Entities.SubEntities;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Events.Interview.Dtos;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.InterviewEntities;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.Invariants;

namespace WB.Core.SharedKernels.DataCollection.Implementation.Aggregates
{
    public partial class Interview
    {
        public void AnswerPictureQuestion(Guid userId, Guid questionId, RosterVector rosterVector, DateTime answerTime, string pictureFileName)
        {
            new InterviewPropertiesInvariants(this.properties).RequireAnswerCanBeChanged();

            var answeredQuestion = new Identity(questionId, rosterVector);

            IQuestionnaire questionnaire = this.GetQuestionnaireOrThrow(this.questionnaireId, this.questionnaireVersion, this.language);

            var tree = this.BuildInterviewTree(questionnaire);
            var treeInvariants = new InterviewTreeInvariants(tree);

            this.ThrowIfQuestionDoesNotExist(questionId, questionnaire);
            treeInvariants.RequireRosterVectorQuestionInstanceExists(questionId, rosterVector);
            this.ThrowIfQuestionTypeIsNotOneOfExpected(questionId, questionnaire, QuestionType.Multimedia);
            treeInvariants.RequireQuestionIsEnabled(answeredQuestion);

            var expressionProcessorState = this.GetClonedExpressionState();

            this.CalculateInterviewChangesOnAnswerPictureQuestion(expressionProcessorState, userId, questionId, rosterVector, answerTime, pictureFileName, questionnaire);
        }
        private void CalculateInterviewChangesOnAnswerPictureQuestion(ILatestInterviewExpressionState expressionProcessorState, Guid userId, Guid questionId, RosterVector rosterVector,
            DateTime answerTime, string pictureFileName, IQuestionnaire questionnaire)
        {
            expressionProcessorState.UpdateMediaAnswer(questionId, rosterVector, pictureFileName);

            InterviewChanges interviewChanges = this.CalculateInterviewChangesOnAnswerQuestion(userId, questionId, rosterVector, pictureFileName,
                AnswerChangeType.Picture, answerTime, questionnaire, expressionProcessorState);

            this.ApplyInterviewChanges(interviewChanges);
        }
    }
}