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
        public void AnswerPictureQuestion(Guid userId, Guid questionId, RosterVector rosterVector, DateTime answerTime, string pictureFileName)
        {
            new InterviewPropertiesInvariants(this.properties).RequireAnswerCanBeChanged();

            var answeredQuestion = new Identity(questionId, rosterVector);

            IQuestionnaire questionnaire = this.GetQuestionnaireOrThrow();

            var treeInvariants = new InterviewTreeInvariants(this.Tree);

            this.ThrowIfQuestionDoesNotExist(questionId, questionnaire);
            treeInvariants.RequireRosterVectorQuestionInstanceExists(questionId, rosterVector);
            treeInvariants.RequireQuestionIsEnabled(answeredQuestion);

            this.ThrowIfQuestionTypeIsNotOneOfExpected(questionId, questionnaire, QuestionType.Multimedia);

            var changedInterviewTree = this.Tree.Clone();

            changedInterviewTree.GetQuestion(answeredQuestion).AsMultimedia.SetAnswer(MultimediaAnswer.FromString(pictureFileName));

            this.UpdateTreeWithDependentChanges(changedInterviewTree, new [] { answeredQuestion }, questionnaire);
            var treeDifference = FindDifferenceBetweenTrees(this.Tree, changedInterviewTree);

            this.ApplyEvents(treeDifference, userId);
        }
    }
}