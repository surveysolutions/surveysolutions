using System;
using System.Collections.Generic;
using Main.Core.Entities.SubEntities;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.InterviewEntities;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.InterviewEntities.Answers;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.Invariants;

namespace WB.Core.SharedKernels.DataCollection.Implementation.Aggregates
{
    public partial class Interview
    {
        public void AnswerQRBarcodeQuestion(Guid userId, Guid questionId, RosterVector rosterVector, DateTime answerTime, string answer)
        {
            new InterviewPropertiesInvariants(this.properties)
                .RequireAnswerCanBeChanged();

            var questionIdentity = new Identity(questionId, rosterVector);

            IQuestionnaire questionnaire = this.GetQuestionnaireOrThrow();

            new InterviewQuestionInvariants(questionIdentity, questionnaire, this.Tree)
                .RequireQuestion(QuestionType.QRBarcode)
                .RequireQuestionInstanceExists()
                .RequireQuestionIsEnabled();

            var changedInterviewTree = this.Tree.Clone();
            changedInterviewTree.GetQuestion(questionIdentity).AsQRBarcode.SetAnswer(QRBarcodeAnswer.FromString(answer));

            this.UpdateTreeWithDependentChanges(changedInterviewTree, new [] { questionIdentity }, questionnaire);
            var treeDifference = FindDifferenceBetweenTrees(this.Tree, changedInterviewTree);

            this.ApplyEvents(treeDifference, userId);
        }
    }
}