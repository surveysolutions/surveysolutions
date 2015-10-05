using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.SharedKernels.DataCollection.Events.Interview.Dtos;
using WB.Core.SharedKernels.DataCollection.Utils;

namespace WB.Core.SharedKernels.DataCollection.Implementation.Aggregates
{
    public class InterviewStateDependentOnAnswers
    {
        public InterviewStateDependentOnAnswers()
        {
            this.AnswersSupportedInExpressions = new ConcurrentDictionary<string, object>();
            this.LinkedSingleOptionAnswersBuggy = new ConcurrentDictionary<string, Tuple<Guid, decimal[], decimal[]>>();
            this.LinkedMultipleOptionsAnswers = new ConcurrentDictionary<string, Tuple<Guid, decimal[], decimal[][]>>();
            this.TextListAnswers = new ConcurrentDictionary<string, Tuple<decimal, string>[]>();

            this.AnsweredQuestions = new ConcurrentHashSet<string>();
            this.DisabledGroups = new ConcurrentHashSet<string>();
            this.DisabledQuestions = new ConcurrentHashSet<string>();
            this.RosterGroupInstanceIds = new ConcurrentDictionary<string, DistinctDecimalList>();
            this.ValidAnsweredQuestions = new ConcurrentHashSet<string>();
            this.InvalidAnsweredQuestions = new ConcurrentHashSet<string>();
            this.AnswerComments = new ConcurrentBag<AnswerComment>();
        }

        public ConcurrentDictionary<string, object> AnswersSupportedInExpressions { set; get; }
        public ConcurrentDictionary<string, Tuple<Guid, decimal[], decimal[]>> LinkedSingleOptionAnswersBuggy { set; get; }
        public ConcurrentDictionary<string, Tuple<Guid, decimal[], decimal[][]>> LinkedMultipleOptionsAnswers { set; get; }
        public ConcurrentDictionary<string, Tuple<decimal, string>[]> TextListAnswers { set; get; }
        public ConcurrentHashSet<string> AnsweredQuestions { set; get; }
        public ConcurrentHashSet<string> DisabledGroups { set; get; }
        public ConcurrentHashSet<string> DisabledQuestions { set; get; }
        public ConcurrentDictionary<string, DistinctDecimalList> RosterGroupInstanceIds { set; get; }
        public ConcurrentHashSet<string> ValidAnsweredQuestions { set; get; }
        public ConcurrentHashSet<string> InvalidAnsweredQuestions { set; get; }
        public ConcurrentBag<AnswerComment> AnswerComments { get; set; }

        public void ApplyInterviewChanges(InterviewChanges changes)
        {
            if (changes.EnablementChanges != null)
                this.ApplyEnablementChanges(changes.EnablementChanges);

            if (changes.ValidityChanges != null)
            {
                this.DeclareAnswersInvalid(changes.ValidityChanges.AnswersDeclaredInvalid);
            }

            if (changes.RosterCalculationData != null)
            {
                this.ApplyRosterData(changes.RosterCalculationData);
            }

            if (changes.AnswersForLinkedQuestionsToRemove != null)
            {
                this.RemoveAnswers(changes.AnswersForLinkedQuestionsToRemove);
            }
        }

        public void ApplyRosterData(RosterCalculationData rosterCalculationData)
        {
            if (rosterCalculationData.RosterInstancesToAdd.Any())
            {
                AddedRosterInstance[] instances = rosterCalculationData
                    .RosterInstancesToAdd
                    .Select(roster => new AddedRosterInstance(roster.GroupId, roster.OuterRosterVector, roster.RosterInstanceId, roster.SortIndex))
                    .ToArray();

                this.AddRosterInstances(instances);
            }

            if (rosterCalculationData.RosterInstancesToRemove.Any())
            {
                RosterInstance[] instances = rosterCalculationData
                    .RosterInstancesToRemove
                    .Select(roster => new RosterInstance(roster.GroupId, roster.OuterRosterVector, roster.RosterInstanceId))
                    .ToArray();

                this.RemoveRosterInstances(instances);
            }

            if (rosterCalculationData.AnswersToRemoveByDecreasedRosterSize.Any())
            {
                this.RemoveAnswers(rosterCalculationData.AnswersToRemoveByDecreasedRosterSize);
            }

            rosterCalculationData.RosterInstantiatesFromNestedLevels.ForEach(this.ApplyRosterData);
        }

        public void ApplyEnablementChanges(EnablementChanges enablementChanges)
        {
            this.EnableGroups(enablementChanges.GroupsToBeEnabled);
            this.DisableGroups(enablementChanges.GroupsToBeDisabled);
            this.EnableQuestions(enablementChanges.QuestionsToBeEnabled);
            this.DisableQuestions(enablementChanges.QuestionsToBeDisabled);
        }

        public void AddRosterInstances(AddedRosterInstance[] instances)
        {
            foreach (var instance in instances)
            {
                string rosterGroupKey = ConversionHelper.ConvertIdAndRosterVectorToString(instance.GroupId, instance.OuterRosterVector);
                DistinctDecimalList rosterRowInstances = RosterGroupInstanceIds.ContainsKey(rosterGroupKey)
                    ? RosterGroupInstanceIds[rosterGroupKey]
                    : new DistinctDecimalList();

                rosterRowInstances.Add(instance.RosterInstanceId);

                RosterGroupInstanceIds[rosterGroupKey] = rosterRowInstances;
            }
        }

        public void RemoveRosterInstances(IEnumerable<RosterInstance> instances)
        {
            foreach (var instance in instances)
            {
                string rosterGroupKey = ConversionHelper.ConvertIdAndRosterVectorToString(instance.GroupId, instance.OuterRosterVector);

                var rosterRowInstances = RosterGroupInstanceIds.ContainsKey(rosterGroupKey)
                    ? RosterGroupInstanceIds[rosterGroupKey]
                    : new DistinctDecimalList();
                rosterRowInstances.Remove(instance.RosterInstanceId);

                RosterGroupInstanceIds[rosterGroupKey] = rosterRowInstances;
            }
        }

        public void EnableQuestions(IEnumerable<Identity> groups)
        {
            foreach (string questionKey in groups.Select(ConvertEventIdentityToString))
            {
                DisabledQuestions.Remove(questionKey);
            }
        }

        public void DisableQuestions(IEnumerable<Identity> groups)
        {
            foreach (string questionKey in groups.Select(ConvertEventIdentityToString))
            {
                DisabledQuestions.Add(questionKey);
            }
        }

        public void EnableGroups(IEnumerable<Identity> groups)
        {
            foreach (string groupKey in groups.Select(ConvertEventIdentityToString))
            {
                DisabledGroups.Remove(groupKey);
            }
        }

        public void DisableGroups(IEnumerable<Identity> groups)
        {
            foreach (string groupKey in groups.Select(ConvertEventIdentityToString))
            {
                this.DisabledGroups.Add(groupKey);
            }
        }

        public void DeclareAnswersInvalid(IEnumerable<Identity> questions)
        {
            foreach (string questionKey in questions.Select(ConvertEventIdentityToString))
            {
                this.ValidAnsweredQuestions.Remove(questionKey);
                this.InvalidAnsweredQuestions.Add(questionKey);
            }
        }

        public void DeclareAnswersValid(IEnumerable<Identity> questions)
        {
            foreach (string questionKey in questions.Select(ConvertEventIdentityToString))
            {
                this.ValidAnsweredQuestions.Add(questionKey);
                this.InvalidAnsweredQuestions.Remove(questionKey);
            }
        }
        
        public void RemoveAnswers(IEnumerable<Identity> questions)
        {
            foreach (string questionKey in questions.Select(ConvertEventIdentityToString))
            {
                this.AnswersSupportedInExpressions.Remove(questionKey);
                this.LinkedSingleOptionAnswersBuggy.Remove(questionKey);
                this.LinkedMultipleOptionsAnswers.Remove(questionKey);
                this.TextListAnswers.Remove(questionKey);
                this.AnsweredQuestions.Remove(questionKey);
                this.DisabledQuestions.Remove(questionKey);
                this.ValidAnsweredQuestions.Remove(questionKey);
                this.InvalidAnsweredQuestions.Remove(questionKey);
            }
        }

        /// <remarks>
        /// The opposite operation (get id or vector from string) should never be performed!
        /// This is one-way transformation. Opposite operation is too slow.
        /// If you need to compactify data and get it back, you should use another datatype, not a string.
        /// </remarks>
        private static string ConvertEventIdentityToString(Identity identity)
        {
            return ConversionHelper.ConvertIdAndRosterVectorToString(identity.Id, identity.RosterVector);
        }
    }
}