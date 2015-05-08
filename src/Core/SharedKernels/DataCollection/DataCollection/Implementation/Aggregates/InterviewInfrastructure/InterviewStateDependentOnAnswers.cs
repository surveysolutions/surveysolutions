using System;
using System.Collections.Generic;
using System.Linq;
using WB.Core.GenericSubdomains.Utils;
using WB.Core.SharedKernels.DataCollection.Events.Interview.Dtos;

namespace WB.Core.SharedKernels.DataCollection.Implementation.Aggregates
{
    public class InterviewStateDependentOnAnswers
    {
        public InterviewStateDependentOnAnswers()
        {
            this.AnswersSupportedInExpressions = new Dictionary<string, object>();
            this.LinkedSingleOptionAnswersBuggy = new Dictionary<string, Tuple<Guid, decimal[], decimal[]>>();
            this.LinkedMultipleOptionsAnswers = new Dictionary<string, Tuple<Guid, decimal[], decimal[][]>>();
            this.TextListAnswers = new Dictionary<string, Tuple<decimal, string>[]>();

            this.AnsweredQuestions = new HashSet<string>();
            this.DisabledGroups = new HashSet<string>();
            this.DisabledQuestions = new HashSet<string>();
            this.RosterGroupInstanceIds = new Dictionary<string, DistinctDecimalList>();
            this.ValidAnsweredQuestions = new HashSet<string>();
            this.InvalidAnsweredQuestions = new HashSet<string>();
            this.AnswerComments = new List<AnswerComment>();
        }

        public Dictionary<string, object> AnswersSupportedInExpressions { set; get; }
        public Dictionary<string, Tuple<Guid, decimal[], decimal[]>> LinkedSingleOptionAnswersBuggy { set; get; }
        public Dictionary<string, Tuple<Guid, decimal[], decimal[][]>> LinkedMultipleOptionsAnswers { set; get; }
        public Dictionary<string, Tuple<decimal, string>[]> TextListAnswers { set; get; }
        public HashSet<string> AnsweredQuestions { set; get; }
        public HashSet<string> DisabledGroups { set; get; }
        public HashSet<string> DisabledQuestions { set; get; }
        public Dictionary<string, DistinctDecimalList> RosterGroupInstanceIds { set; get; }
        public HashSet<string> ValidAnsweredQuestions { set; get; }
        public HashSet<string> InvalidAnsweredQuestions { set; get; }
        public List<AnswerComment> AnswerComments { get; set; }

        public void ApplyInterviewChanges(InterviewChanges changes)
        {
            if (changes.EnablementChanges != null)
                this.ApplyEnablementChanges(changes.EnablementChanges);

            if (changes.ValidityChanges != null)
            {
                this.DeclareAnswersInvalid(Events.Interview.Dtos.Identity.ToEventIdentities(changes.ValidityChanges.AnswersDeclaredInvalid));
            }

            if (changes.RosterCalculationData != null)
            {
                this.ApplyRosterData(changes.RosterCalculationData);
            }

            if (changes.AnswersForLinkedQuestionsToRemove != null)
            {
                this.RemoveAnswers(Events.Interview.Dtos.Identity.ToEventIdentities(changes.AnswersForLinkedQuestionsToRemove));
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
                this.RemoveAnswers(Events.Interview.Dtos.Identity.ToEventIdentities(rosterCalculationData.AnswersToRemoveByDecreasedRosterSize));
            }

            rosterCalculationData.RosterInstantiatesFromNestedLevels.ForEach(this.ApplyRosterData);
        }

        public void ApplyEnablementChanges(EnablementChanges enablementChanges)
        {
            this.EnableGroups(Events.Interview.Dtos.Identity.ToEventIdentities(enablementChanges.GroupsToBeEnabled));
            this.DisableGroups(Events.Interview.Dtos.Identity.ToEventIdentities(enablementChanges.GroupsToBeDisabled));
            this.EnableQuestions(Events.Interview.Dtos.Identity.ToEventIdentities(enablementChanges.QuestionsToBeEnabled));
            this.DisableQuestions(Events.Interview.Dtos.Identity.ToEventIdentities(enablementChanges.QuestionsToBeDisabled));
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

        public void EnableQuestions(IEnumerable<Events.Interview.Dtos.Identity> groups)
        {
            foreach (string questionKey in groups.Select(ConvertEventIdentityToString))
            {
                DisabledQuestions.Remove(questionKey);
            }
        }

        public void DisableQuestions(IEnumerable<Events.Interview.Dtos.Identity> groups)
        {
            foreach (string questionKey in groups.Select(ConvertEventIdentityToString))
            {
                DisabledQuestions.Add(questionKey);
            }
        }

        public void EnableGroups(IEnumerable<Events.Interview.Dtos.Identity> groups)
        {
            foreach (string groupKey in groups.Select(ConvertEventIdentityToString))
            {
                DisabledGroups.Remove(groupKey);
            }
        }

        public void DisableGroups(IEnumerable<Events.Interview.Dtos.Identity> groups)
        {
            foreach (string groupKey in groups.Select(ConvertEventIdentityToString))
            {
                this.DisabledGroups.Add(groupKey);
            }
        }

        public void DeclareAnswersInvalid(IEnumerable<Events.Interview.Dtos.Identity> questions)
        {
            foreach (string questionKey in questions.Select(ConvertEventIdentityToString))
            {
                this.ValidAnsweredQuestions.Remove(questionKey);
                this.InvalidAnsweredQuestions.Add(questionKey);
            }
        }

        public void DeclareAnswersValid(IEnumerable<Events.Interview.Dtos.Identity> questions)
        {
            foreach (string questionKey in questions.Select(ConvertEventIdentityToString))
            {
                this.ValidAnsweredQuestions.Add(questionKey);
                this.InvalidAnsweredQuestions.Remove(questionKey);
            }
        }
        
        public void RemoveAnswers(IEnumerable<Events.Interview.Dtos.Identity> questions)
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
        private static string ConvertEventIdentityToString(Events.Interview.Dtos.Identity identity)
        {
            return ConversionHelper.ConvertIdAndRosterVectorToString(identity.Id, identity.RosterVector);
        }
    }
}