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
        public void AnswerSingleOptionLinkedQuestion(Guid userId, Guid questionId, RosterVector rosterVector, DateTime answerTime, decimal[] selectedRosterVector)
        {
            new InterviewPropertiesInvariants(this.properties).RequireAnswerCanBeChanged();

            var answeredQuestion = new Identity(questionId, rosterVector);

            IQuestionnaire questionnaire = this.GetQuestionnaireOrThrow();

            this.CheckLinkedSingleOptionQuestionInvariants(questionId, rosterVector, selectedRosterVector, questionnaire, answeredQuestion, this.Tree);

            var changedInterviewTree = this.Tree.Clone();

            var changedQuestionIdentities = new List<Identity> { answeredQuestion };
            changedInterviewTree.GetQuestion(answeredQuestion).AsSingleLinkedOption.SetAnswer(CategoricalLinkedSingleOptionAnswer.FromRosterVector(selectedRosterVector));

            this.CalculateTreeDiffChanges(changedInterviewTree, questionnaire, changedQuestionIdentities);

            this.ApplyEvents(changedInterviewTree, userId);
        }
    }
}