using System;
using System.Collections.Generic;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.Invariants;

namespace WB.Core.SharedKernels.DataCollection.Implementation.Aggregates
{
    public partial class Interview
    {
        public void RemoveAnswer(Guid questionId, RosterVector rosterVector, Guid userId, DateTime removeTime)
        {
            new InterviewPropertiesInvariants(this.properties).RequireAnswerCanBeChanged();

            var questionIdentity = new Identity(questionId, rosterVector);

            IQuestionnaire questionnaire = this.GetQuestionnaireOrThrow(this.questionnaireId, this.questionnaireVersion, this.language);

            var sourceInterviewTree = this.BuildInterviewTree(questionnaire, this.interviewState);

            var treeInvariants = new InterviewTreeInvariants(sourceInterviewTree);
            this.ThrowIfQuestionDoesNotExist(questionId, questionnaire);
            treeInvariants.RequireRosterVectorQuestionInstanceExists(questionId, rosterVector);
            treeInvariants.RequireQuestionIsEnabled(questionIdentity);

            var changedInterviewTree = sourceInterviewTree.Clone();

            var changedQuestionIdentities = new List<Identity> { questionIdentity };
            changedInterviewTree.GetQuestion(questionIdentity).RemoveAnswer();

            RemoveAnswersForDependendCascadingQuestions(questionIdentity, changedInterviewTree, questionnaire, changedQuestionIdentities);

            this.ApplyQuestionAnswer(userId, changedInterviewTree, questionnaire, changedQuestionIdentities, sourceInterviewTree);
        }
    }
}