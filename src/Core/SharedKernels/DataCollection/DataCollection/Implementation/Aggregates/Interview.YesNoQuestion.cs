using System;
using System.Collections.Generic;
using System.Linq;
using Main.Core.Entities.SubEntities;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Commands.Interview;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Events.Interview.Dtos;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.InterviewEntities;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.Invariants;

namespace WB.Core.SharedKernels.DataCollection.Implementation.Aggregates
{
    public partial class Interview
    {
        public void AnswerYesNoQuestion(AnswerYesNoQuestion command)
        {
            new InterviewPropertiesInvariants(this.properties).RequireAnswerCanBeChanged();

            var answeredQuestion = new Identity(command.QuestionId, command.RosterVector);

            IQuestionnaire questionnaire = this.GetQuestionnaireOrThrow(this.questionnaireId, this.questionnaireVersion, this.language);

            var sourceInterviewTree = this.BuildInterviewTree(questionnaire, this.interviewState);

            this.CheckYesNoQuestionInvariants(command.Question, command.AnsweredOptions, questionnaire, this.interviewState);

            var changedInterviewTree = this.BuildInterviewTree(questionnaire, this.interviewState);

            var changedQuestionIdentities = new List<Identity> { answeredQuestion };
            changedInterviewTree.GetQuestion(answeredQuestion).AsYesNo.SetAnswer(command.AnsweredOptions);

            this.ApplyQuestionAnswer(command.UserId, changedInterviewTree, questionnaire, changedQuestionIdentities, sourceInterviewTree);
        }

        private InterviewChanges CalculateInterviewChangesOnYesNoQuestionAnswer(Guid questionId, RosterVector rosterVector, AnsweredYesNoOption[] answer, DateTime answerTime, Guid userId, IQuestionnaire questionnaire,
            ILatestInterviewExpressionState expressionProcessorState, IReadOnlyInterviewStateDependentOnAnswers state, Func<IReadOnlyInterviewStateDependentOnAnswers, Identity, object> getAnswer)
        {
            List<decimal> availableValues = questionnaire.GetMultiSelectAnswerOptionsAsValues(questionId).ToList();
            IEnumerable<decimal> rosterInstanceIds = answer.Where(answeredOption => answeredOption.Yes).Select(answeredOption => answeredOption.OptionValue).ToList();

            Dictionary<decimal, int?> rosterInstanceIdsWithSortIndexes;
            if (!questionnaire.ShouldQuestionRecordAnswersOrder(questionId))
            {
                Dictionary<decimal, int?> indexedSelectedOptionsMap = availableValues
                    .Where(x => rosterInstanceIds.Any(optionValue => optionValue == x))
                    .Select((x, i) => new { OptionValue = x, Index = i })
                    .ToDictionary(x => x.OptionValue, x => (int?)x.Index);

                rosterInstanceIdsWithSortIndexes = rosterInstanceIds.ToDictionary(
                    selectedValue => selectedValue,
                    selectedValue => indexedSelectedOptionsMap[selectedValue]);
            }
            else
            {
                int? orderPosition = 0;
                rosterInstanceIdsWithSortIndexes = rosterInstanceIds.ToDictionary(
                    selectedValue => selectedValue,
                    selectedValue => orderPosition++);
            }


            var interviewByAnswerChange = new List<AnswerChange>
            {
                new AnswerChange(AnswerChangeType.YesNo, userId, questionId, rosterVector, answerTime, answer)
            };

            expressionProcessorState.UpdateYesNoAnswer(questionId, rosterVector, ConvertToYesNoAnswersOnly(answer));

            return this.EmitInterviewChangesForMultioptionQuestion(questionId, rosterVector, answerTime, userId, questionnaire, expressionProcessorState, state, getAnswer,
                rosterInstanceIdsWithSortIndexes, interviewByAnswerChange, rosterInstanceIds);
        }
    }
}