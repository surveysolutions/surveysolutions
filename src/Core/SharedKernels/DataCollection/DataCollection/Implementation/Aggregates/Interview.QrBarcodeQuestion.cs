using System;
using System.Collections.Generic;
using Main.Core.Entities.SubEntities;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.Invariants;

namespace WB.Core.SharedKernels.DataCollection.Implementation.Aggregates
{
    public partial class Interview
    {
        private InterviewChanges CalculateInterviewChangesOnAnswerQRBarcodeQuestion(ILatestInterviewExpressionState expressionProcessorState,
            Guid userId, Guid questionId, RosterVector rosterVector, DateTime answerTime, string answer, IQuestionnaire questionnaire)
        {
            expressionProcessorState.UpdateQrBarcodeAnswer(questionId, rosterVector, answer);

            return this.CalculateInterviewChangesOnAnswerQuestion(userId, questionId, rosterVector, answer, AnswerChangeType.QRBarcode, answerTime, questionnaire, expressionProcessorState);
        }

        public void AnswerQRBarcodeQuestion(Guid userId, Guid questionId, RosterVector rosterVector, DateTime answerTime, string answer)
        {
            new InterviewPropertiesInvariants(this.properties).RequireAnswerCanBeChanged();

            var answeredQuestion = new Identity(questionId, rosterVector);

            IQuestionnaire questionnaire = this.GetQuestionnaireOrThrow(this.questionnaireId, this.questionnaireVersion, this.language);

            var sourceInterviewTree = this.BuildInterviewTree(questionnaire);
            var treeInvariants = new InterviewTreeInvariants(sourceInterviewTree);

            this.ThrowIfQuestionDoesNotExist(questionId, questionnaire);
            treeInvariants.RequireRosterVectorQuestionInstanceExists(questionId, rosterVector);

            this.ThrowIfQuestionTypeIsNotOneOfExpected(questionId, questionnaire, QuestionType.QRBarcode);
            treeInvariants.RequireQuestionIsEnabled(answeredQuestion);

            var changedInterviewTree = this.BuildInterviewTree(questionnaire, this.interviewState);
            var changedQuestionIdentities = new List<Identity> { answeredQuestion };
            changedInterviewTree.GetQuestion(answeredQuestion).AsQRBarcode.SetAnswer(answer);

            this.ApplyQuestionAnswer(userId, changedInterviewTree, questionnaire, changedQuestionIdentities, sourceInterviewTree);
            
        }
    }
}