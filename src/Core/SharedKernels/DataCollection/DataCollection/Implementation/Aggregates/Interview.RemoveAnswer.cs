using System;
using System.Collections.Generic;
using System.Linq;
using Main.Core.Entities.SubEntities;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Events.Interview.Dtos;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.InterviewEntities;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.Invariants;

namespace WB.Core.SharedKernels.DataCollection.Implementation.Aggregates
{
    public partial class Interview
    {
        public void RemoveAnswer(Guid questionId, RosterVector rosterVector, Guid userId, DateTime removeTime)
        {
            new InterviewPropertiesInvariants(this.properties).RequireAnswerCanBeChanged();

            var answeredQuestion = new Identity(questionId, rosterVector);

            IQuestionnaire questionnaire = this.GetQuestionnaireOrThrow(this.questionnaireId, this.questionnaireVersion, this.language);

            var tree = this.BuildInterviewTree(questionnaire);
            var treeInvariants = new InterviewTreeInvariants(tree);

            this.ThrowIfQuestionDoesNotExist(questionId, questionnaire);
            treeInvariants.RequireRosterVectorQuestionInstanceExists(questionId, rosterVector);
            treeInvariants.RequireQuestionIsEnabled(answeredQuestion);

            ILatestInterviewExpressionState expressionProcessorState = this.ExpressionProcessorStatePrototype.Clone();

            InterviewChanges interviewChanges = this.CalculateInterviewChangesOnAnswerRemove(this.interviewState,
                userId, questionId, rosterVector, removeTime, questionnaire, expressionProcessorState);

            ValidityChanges validationChanges = expressionProcessorState.ProcessValidationExpressions();

            this.ApplyInterviewChanges(interviewChanges);
            this.ApplyValidityChangesEvents(validationChanges);
        }

        private InterviewChanges CalculateInterviewChangesOnAnswerRemove(
            IReadOnlyInterviewStateDependentOnAnswers state, Guid userId, Guid questionId,
            RosterVector rosterVector, DateTime removeTime, IQuestionnaire questionnaire,
            ILatestInterviewExpressionState expressionProcessorState)
        {
            List<Guid> rosterIds = questionnaire.GetRosterGroupsByRosterSizeQuestion(questionId).ToList();
            Func<Guid, decimal[], bool> isRoster = (groupId, groupOuterScopeRosterVector)
                => rosterIds.Contains(groupId)
                    && AreEqualRosterVectors(groupOuterScopeRosterVector, rosterVector);

            var rosterInstanceIds = Enumerable.Empty<decimal>();
            RosterCalculationData rosterCalculationData = this.CalculateRosterData(state, rosterIds, rosterVector, rosterInstanceIds, null, questionnaire, (s, i) => null);

            expressionProcessorState.SaveAllCurrentStatesAsPrevious();
            //Update State
            RemoveAnswerFromExpressionProcessorState(expressionProcessorState, questionId, rosterVector);

            var answersToRemoveByCascading = this.GetQuestionsToRemoveAnswersFromDependingOnCascading(questionId, rosterVector, questionnaire, state).ToArray();

            expressionProcessorState.DisableQuestions(answersToRemoveByCascading);

            var rosterInstancesToRemove = this.GetOrderedUnionOfUniqueRosterDataPropertiesByRosterAndNestedRosters(
                d => d.RosterInstancesToRemove, new RosterIdentityComparer(), rosterCalculationData);

            rosterInstancesToRemove.ForEach(r => expressionProcessorState.RemoveRoster(r.GroupId, r.OuterRosterVector, r.RosterInstanceId));

            var interviewByAnswerChange = new List<AnswerChange>
            {
                new AnswerChange(AnswerChangeType.RemoveAnswer, userId, questionId, rosterVector, removeTime, null)
            };

            IReadOnlyInterviewStateDependentOnAnswers alteredState = state.Amend(getRosterInstanceIds: (groupId, groupOuterRosterVector)
                => isRoster(groupId, groupOuterRosterVector)
                    ? rosterInstanceIds
                    : state.GetRosterInstanceIds(groupId, groupOuterRosterVector));

            var changes = this.CalculateInterviewStructuralChanges(questionId, rosterVector, removeTime, userId, questionnaire, expressionProcessorState,
                interviewByAnswerChange, rosterCalculationData, alteredState);

            changes.AnswersToRemove.AddRange(answersToRemoveByCascading);

            return changes;
        }
    }
}