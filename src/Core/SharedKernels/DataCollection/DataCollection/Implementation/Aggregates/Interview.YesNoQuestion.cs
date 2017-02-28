using System;
using System.Collections.Generic;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Commands.Interview;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.InterviewEntities;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.InterviewEntities.Answers;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.Invariants;

namespace WB.Core.SharedKernels.DataCollection.Implementation.Aggregates
{
    public partial class Interview
    {
        public void AnswerYesNoQuestion(AnswerYesNoQuestion command)
        {
            new InterviewPropertiesInvariants(this.properties)
                .RequireAnswerCanBeChanged();

            var questionIdentity = new Identity(command.QuestionId, command.RosterVector);

            IQuestionnaire questionnaire = this.GetQuestionnaireOrThrow();

            new InterviewQuestionInvariants(command.Question, questionnaire, this.Tree)
                .RequireYesNoAnswerAllowed(YesNoAnswer.FromAnsweredYesNoOptions(command.AnsweredOptions));

            var changedInterviewTree = this.Tree.Clone();

            changedInterviewTree.GetQuestion(questionIdentity).AsYesNo.SetAnswer(YesNoAnswer.FromAnsweredYesNoOptions(command.AnsweredOptions));

            changedInterviewTree.ActualizeTree();

            this.UpdateTreeWithDependentChanges(changedInterviewTree, new [] { questionIdentity }, questionnaire);
            var treeDifference = FindDifferenceBetweenTrees(this.Tree, changedInterviewTree);

            this.ApplyEvents(treeDifference, command.UserId);
        }
    }
}