using System;
using System.Collections.Generic;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.InterviewEntities.Answers;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.Invariants;

namespace WB.Core.SharedKernels.DataCollection.Implementation.Aggregates
{
    public partial class Interview
    {
        public void AnswerMultipleOptionsLinkedQuestion(Guid userId, Guid questionId, RosterVector rosterVector, DateTime answerTime, decimal[][] selectedRosterVectors)
        {
            new InterviewPropertiesInvariants(this.properties).RequireAnswerCanBeChanged();

            var answeredQuestion = new Identity(questionId, rosterVector);

            IQuestionnaire questionnaire = this.GetQuestionnaireOrThrow();

            var sourceInterviewTree = this.Tree;

            this.CheckLinkedMultiOptionQuestionInvariants(questionId, rosterVector, selectedRosterVectors, questionnaire, answeredQuestion, sourceInterviewTree);

            var changedInterviewTree = sourceInterviewTree.Clone();

            var changedQuestionIdentities = new List<Identity> { answeredQuestion };
            changedInterviewTree.GetQuestion(answeredQuestion).AsMultiLinkedOption.SetAnswer(CategoricalLinkedMultiOptionAnswer.FromDecimalArrayArray(selectedRosterVectors));

            this.ApplyTreeDiffChanges(userId, changedInterviewTree, questionnaire, changedQuestionIdentities, sourceInterviewTree);
        }
    }
}