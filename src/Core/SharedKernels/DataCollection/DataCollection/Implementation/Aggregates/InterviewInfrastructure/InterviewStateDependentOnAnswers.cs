using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.InteropServices;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Portable.CustomCollections;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Events.Interview.Dtos;
using WB.Core.SharedKernels.DataCollection.Utils;

namespace WB.Core.SharedKernels.DataCollection.Implementation.Aggregates
{
    public class InterviewStateDependentOnAnswers : IReadOnlyInterviewStateDependentOnAnswers 
    {
        private class AmendingWrapper : IReadOnlyInterviewStateDependentOnAnswers
        {
            private readonly IReadOnlyInterviewStateDependentOnAnswers actualState;
            private readonly Func<Guid, RosterVector, IEnumerable<decimal>> getRosterInstanceIds;

            public AmendingWrapper(IReadOnlyInterviewStateDependentOnAnswers actualState, Func<Guid, RosterVector, IEnumerable<decimal>> getRosterInstanceIds)
            {
                this.actualState = actualState;
                this.getRosterInstanceIds = getRosterInstanceIds;
            }

            public IReadOnlyInterviewStateDependentOnAnswers Amend(Func<Guid, RosterVector, IEnumerable<decimal>> getRosterInstanceIds) => this.actualState.Amend(getRosterInstanceIds);

            public bool IsGroupDisabled(Identity @group) => this.actualState.IsGroupDisabled(@group);

            public bool IsStaticTextDisabled(Identity @group) => this.actualState.IsStaticTextDisabled(@group);

            public bool IsQuestionDisabled(Identity question) => this.actualState.IsQuestionDisabled(question);
            public bool IsVariableDisabled(Identity variable) => this.actualState.IsVariableDisabled(variable);

            public bool WasQuestionAnswered(Identity question) => this.actualState.WasQuestionAnswered(question);

            public object GetAnswerSupportedInExpressions(Identity question) => this.actualState.GetAnswerSupportedInExpressions(question);

            public Tuple<decimal, string>[] GetTextListAnswer(Identity question) => this.actualState.GetTextListAnswer(question);

            public ReadOnlyCollection<decimal> GetRosterInstanceIds(Guid groupId, RosterVector outerRosterVector)
            {
                return this.getRosterInstanceIds != null
                    ? this.getRosterInstanceIds(groupId, outerRosterVector).ToReadOnlyCollection()
                    : this.actualState.GetRosterInstanceIds(groupId, outerRosterVector);
            }

            public IEnumerable<Tuple<Identity, RosterVector>> GetAllLinkedToQuestionSingleOptionAnswers(IQuestionnaire questionnaire) => this.actualState.GetAllLinkedToQuestionSingleOptionAnswers(questionnaire);
            public IEnumerable<Tuple<Identity, RosterVector[]>> GetAllLinkedMultipleOptionsAnswers(IQuestionnaire questionnaire) => this.actualState.GetAllLinkedMultipleOptionsAnswers(questionnaire);
            public IEnumerable<Tuple<Identity, RosterVector>> GetAllLinkedToRosterSingleOptionAnswers(IQuestionnaire questionnaire) => this.actualState.GetAllLinkedToRosterSingleOptionAnswers(questionnaire);
            public IEnumerable<Tuple<Identity, RosterVector[]>> GetAllLinkedToRosterMultipleOptionsAnswers(IQuestionnaire questionnaire) => this.actualState.GetAllLinkedToRosterMultipleOptionsAnswers(questionnaire);
            public IReadOnlyCollection<RosterVector> GetOptionsForLinkedQuestion(Identity linkedQuestionIdentity) => this.actualState.GetOptionsForLinkedQuestion(linkedQuestionIdentity);
            public object GetAnswer(Identity identity) => this.actualState.GetAnswer(identity);
        }


        public InterviewStateDependentOnAnswers()
        {
            this.AnswersSupportedInExpressions = new ConcurrentDictionary<string, object>();
            this.LinkedSingleOptionAnswersBuggy = new ConcurrentDictionary<string, Tuple<Identity, RosterVector>>();
            this.LinkedMultipleOptionsAnswers = new ConcurrentDictionary<string, Tuple<Identity, RosterVector[]>>();
            this.LinkedQuestionOptions=new ConcurrentDictionary<Identity, RosterVector[]>();
            this.TextListAnswers = new ConcurrentDictionary<string, Tuple<decimal, string>[]>();

            this.AnsweredQuestions = new ConcurrentHashSet<string>();
            this.DisabledGroups = new ConcurrentHashSet<string>();
            this.DisabledQuestions = new ConcurrentHashSet<string>();
            this.RosterGroupInstanceIds = new ConcurrentDictionary<string, ConcurrentDistinctList<decimal>>();
            this.ValidAnsweredQuestions = new ConcurrentHashSet<Identity>();
            this.InvalidAnsweredQuestions = new ConcurrentDictionary<Identity, IReadOnlyList<FailedValidationCondition>>();
            this.AnswerComments = new ConcurrentBag<AnswerComment>();
            this.RosterTitles = new ConcurrentDictionary<string, string>();
            this.DisabledStaticTexts = new ConcurrentHashSet<Identity>();

            this.ValidStaticTexts = new ConcurrentHashSet<Identity>();
            this.InvalidStaticTexts = new ConcurrentDictionary<Identity, IReadOnlyList<FailedValidationCondition>>();

            this.VariableValues = new ConcurrentDictionary<Identity, object>();
            this.DisabledVariables = new ConcurrentHashSet<Identity>();
        }

        public ConcurrentDictionary<string, object> AnswersSupportedInExpressions { set; get; }
        public ConcurrentDictionary<string, Tuple<Identity, RosterVector>> LinkedSingleOptionAnswersBuggy { set; get; }
        public ConcurrentDictionary<string, Tuple<Identity, RosterVector[]>> LinkedMultipleOptionsAnswers { set; get; }
        public ConcurrentDictionary<Identity, RosterVector[]> LinkedQuestionOptions { set; get; }
        public ConcurrentDictionary<string, Tuple<decimal, string>[]> TextListAnswers { set; get; }
        public ConcurrentHashSet<string> AnsweredQuestions { set; get; }
        public ConcurrentHashSet<string> DisabledGroups { set; get; }
        public ConcurrentHashSet<string> DisabledQuestions { set; get; }
        public ConcurrentHashSet<Identity> DisabledStaticTexts { set; get; }
        public ConcurrentDictionary<string, ConcurrentDistinctList<decimal>> RosterGroupInstanceIds { set; get; }
        public ConcurrentDictionary<string, string> RosterTitles { set; get; }
        public ConcurrentHashSet<Identity> ValidAnsweredQuestions { set; get; }
        public IDictionary<Identity, IReadOnlyList<FailedValidationCondition>> InvalidAnsweredQuestions { set; get; }
        public ConcurrentBag<AnswerComment> AnswerComments { get; set; }

        public ConcurrentHashSet<Identity> ValidStaticTexts { set; get; }
        public IDictionary<Identity, IReadOnlyList<FailedValidationCondition>> InvalidStaticTexts { set; get; }
        public ConcurrentDictionary<Identity, object> VariableValues { set; get; }
        public ConcurrentHashSet<Identity> DisabledVariables { set; get; }
        public bool IsValid { get; set; }

        public InterviewStateDependentOnAnswers Clone()
        {
            return new InterviewStateDependentOnAnswers()
            {
                AnswersSupportedInExpressions = this.AnswersSupportedInExpressions.ToConcurrentDictionary(x=>x.Key,x=>x.Value),
                LinkedSingleOptionAnswersBuggy = this.LinkedSingleOptionAnswersBuggy.ToConcurrentDictionary(x => x.Key, x =>new Tuple<Identity, RosterVector>(x.Value.Item1, x.Value.Item2)),
                LinkedMultipleOptionsAnswers = this.LinkedMultipleOptionsAnswers.ToConcurrentDictionary(x => x.Key, x => new Tuple<Identity, RosterVector[]>(x.Value.Item1, x.Value.Item2.ToArray())),
                LinkedQuestionOptions = this.LinkedQuestionOptions.ToConcurrentDictionary(x => x.Key, x => x.Value.ToArray()),
                TextListAnswers = this.TextListAnswers.ToConcurrentDictionary(x => x.Key, x => x.Value.Select(l=> new Tuple<decimal, string>(l.Item1, l.Item2)).ToArray()),

                AnsweredQuestions = new ConcurrentHashSet<string>(this.AnsweredQuestions),
                DisabledGroups = new ConcurrentHashSet<string>(this.DisabledGroups),
                DisabledQuestions = new ConcurrentHashSet<string>(this.DisabledQuestions),
                RosterGroupInstanceIds = this.RosterGroupInstanceIds.ToConcurrentDictionary(x => x.Key, x => new ConcurrentDistinctList<decimal>(x.Value)),
                ValidAnsweredQuestions = new ConcurrentHashSet<Identity>(this.ValidAnsweredQuestions),
                InvalidAnsweredQuestions = new ConcurrentDictionary<Identity, IReadOnlyList<FailedValidationCondition>>(this.InvalidAnsweredQuestions),
                AnswerComments = new ConcurrentBag<AnswerComment>(this.AnswerComments),
                RosterTitles = this.RosterTitles.ToConcurrentDictionary(x => x.Key, x => x.Value),

                DisabledStaticTexts = new ConcurrentHashSet<Identity>(this.DisabledStaticTexts),
                ValidStaticTexts = new ConcurrentHashSet<Identity>(this.ValidStaticTexts),
                InvalidStaticTexts = new ConcurrentDictionary<Identity, IReadOnlyList<FailedValidationCondition>>(InvalidStaticTexts),

                VariableValues = new ConcurrentDictionary<Identity, object>(this.VariableValues)
            };
        }

        public void ApplyInterviewChanges(InterviewChanges changes)
        {
            if (changes.EnablementChanges != null)
                this.ApplyEnablementChanges(changes.EnablementChanges);

            if (changes.ValidityChanges != null)
            {
                this.DeclareAnswersInvalid(changes.ValidityChanges.FailedValidationConditions.ToDictionary());
            }

            if (changes.RosterCalculationData != null)
            {
                this.ApplyRosterData(changes.RosterCalculationData);
            }

            if (changes.AnswersToRemove != null)
            {
                this.RemoveAnswers(changes.AnswersToRemove);
            }

            if (changes.RosterInstancesWithAffectedTitles != null)
            {
                this.ChangeRosterTitles(
                    changes.RosterInstancesWithAffectedTitles.Select(
                        r =>
                            new ChangedRosterInstanceTitleDto(
                                new RosterInstance(r.Key.Id, r.Key.RosterVector.WithoutLast().ToArray(), r.Key.RosterVector.Last()),
                                r.Value)).ToArray());
            }
            if (changes.LinkedQuestionOptionsChanges != null)
            {
                ApplyLinkedOptionQuestionChanges(changes.LinkedQuestionOptionsChanges);
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

                var changedRosterTitles =
                    rosterCalculationData.RosterInstancesToAdd.Select(
                        i =>
                            new ChangedRosterInstanceTitleDto(
                                new RosterInstance(i.GroupId, i.OuterRosterVector, i.RosterInstanceId),
                                rosterCalculationData.GetRosterInstanceTitle(i.GroupId, i.RosterInstanceId))).ToArray();

                ChangeRosterTitles(changedRosterTitles);
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

        public void ApplyLinkedOptionQuestionChanges(ChangedLinkedOptions[] linkedQuestionOptionsChanges)
        {
            foreach (var linkedQuestionOptionsChange in linkedQuestionOptionsChanges)
            {
                var newLinkedQuestionOptions = linkedQuestionOptionsChange.Options.ToArray();

                this.LinkedQuestionOptions.AddOrUpdate(linkedQuestionOptionsChange.QuestionId, linkedQuestionOptionsChange.Options.ToArray(), (k, v) => newLinkedQuestionOptions);
            }
        }

        public void ApplyEnablementChanges(EnablementChanges enablementChanges)
        {
            this.EnableGroups(enablementChanges.GroupsToBeEnabled);
            this.DisableGroups(enablementChanges.GroupsToBeDisabled);
            this.EnableQuestions(enablementChanges.QuestionsToBeEnabled);
            this.DisableQuestions(enablementChanges.QuestionsToBeDisabled);
        }

        public void ChangeRosterTitles(ChangedRosterInstanceTitleDto[] changedInstances)
        {
            foreach (var changedInstance in changedInstances)
            {
                string rosterGroupKey =
                    ConversionHelper.ConvertIdAndRosterVectorToString(changedInstance.RosterInstance.GroupId,
                        changedInstance.RosterInstance.GetIdentity().RosterVector);

                this.RosterTitles[rosterGroupKey] = changedInstance.Title;
            }
        }

        public void AddRosterInstances(AddedRosterInstance[] instances)
        {
            foreach (var instance in instances)
            {
                string rosterGroupKey = ConversionHelper.ConvertIdAndRosterVectorToString(instance.GroupId, instance.OuterRosterVector);
                var rosterRowInstances = this.RosterGroupInstanceIds.ContainsKey(rosterGroupKey)
                    ? this.RosterGroupInstanceIds[rosterGroupKey]
                    : new ConcurrentDistinctList<decimal>();

                rosterRowInstances.Add(instance.RosterInstanceId);

                this.RosterGroupInstanceIds[rosterGroupKey] = rosterRowInstances;
            }
        }

        public void RemoveRosterInstances(IEnumerable<RosterInstance> instances)
        {
            foreach (var instance in instances)
            {
                string rosterGroupKey = ConversionHelper.ConvertIdAndRosterVectorToString(instance.GroupId, instance.OuterRosterVector);

                var rosterRowInstances = this.RosterGroupInstanceIds.ContainsKey(rosterGroupKey)
                    ? this.RosterGroupInstanceIds[rosterGroupKey]
                    : new ConcurrentDistinctList<decimal>();
                rosterRowInstances.Remove(instance.RosterInstanceId);
                this.DisabledGroups.Remove(rosterGroupKey);

                this.RosterGroupInstanceIds[rosterGroupKey] = rosterRowInstances;
            }
        }

        public void EnableStaticTexts(Identity[] staticTexts)
        {
            foreach (var staticText in staticTexts)
            {
                this.DisabledStaticTexts.Remove(staticText);
            }
        }

        public void DisableStaticTexts(Identity[] staticTexts)
        {
            foreach (var staticText in staticTexts)
            {
                this.DisabledStaticTexts.Add(staticText);
            }
        }

        public void EnableQuestions(IEnumerable<Identity> groups)
        {
            foreach (string questionKey in groups.Select(ConversionHelper.ConvertIdentityToString))
            {
                this.DisabledQuestions.Remove(questionKey);
            }
        }

        public void DisableQuestions(IEnumerable<Identity> groups)
        {
            foreach (string questionKey in groups.Select(ConversionHelper.ConvertIdentityToString))
            {
                this.DisabledQuestions.Add(questionKey);
            }
        }

        public void EnableGroups(IEnumerable<Identity> groups)
        {
            foreach (string groupKey in groups.Select(ConversionHelper.ConvertIdentityToString))
            {
                this.DisabledGroups.Remove(groupKey);
            }
        }

        public void DisableGroups(IEnumerable<Identity> groups)
        {
            foreach (string groupKey in groups.Select(ConversionHelper.ConvertIdentityToString))
            {
                this.DisabledGroups.Add(groupKey);
            }
        }

        public void DeclareAnswersInvalid(IReadOnlyDictionary<Identity, IReadOnlyList<FailedValidationCondition>> questions)
        {
            foreach (var questionKey in questions)
            {
                this.InvalidAnsweredQuestions[questionKey.Key] = questionKey.Value;
                this.ValidAnsweredQuestions.Remove(questionKey.Key);
            }
        }

        public void DeclareAnswersValid(IEnumerable<Identity> questions)
        {
            foreach (var questionKey in questions)
            {
                this.ValidAnsweredQuestions.Add(questionKey);
                this.InvalidAnsweredQuestions.Remove(questionKey);
            }
        }

        public void DeclareStaticTextValid(IEnumerable<Identity> statisTexts)
        {
            foreach (var questionKey in statisTexts)
            {
                this.ValidStaticTexts.Add(questionKey);
                this.InvalidStaticTexts.Remove(questionKey);
            }
        }

        public void DeclareStaticTextInvalid(IReadOnlyDictionary<Identity, IReadOnlyList<FailedValidationCondition>> statisTexts)
        {
            foreach (var questionKey in statisTexts)
            {
                this.InvalidStaticTexts[questionKey.Key] = questionKey.Value;
                this.ValidStaticTexts.Remove(questionKey.Key);
            }
        }

        public void RemoveAnswers(IEnumerable<Identity> questions)
        {
            foreach (var questionKey in questions)
            {
                var identityString = ConversionHelper.ConvertIdentityToString(questionKey);
                this.AnswersSupportedInExpressions.TryRemove(identityString);
                this.LinkedSingleOptionAnswersBuggy.TryRemove(identityString);
                this.LinkedMultipleOptionsAnswers.TryRemove(identityString);
                this.TextListAnswers.TryRemove(identityString);
                this.AnsweredQuestions.Remove(identityString);
                this.DisabledQuestions.Remove(identityString);
                this.ValidAnsweredQuestions.Remove(questionKey);
                this.InvalidAnsweredQuestions.Remove(questionKey);
            }
        }


        public IReadOnlyInterviewStateDependentOnAnswers Amend(Func<Guid, RosterVector, IEnumerable<decimal>> getRosterInstanceIds)
        {
            return new AmendingWrapper(this, getRosterInstanceIds);
        }

        public bool IsStaticTextDisabled(Identity staticText)
        {
            return this.DisabledStaticTexts.Contains(staticText);
        }

        public bool IsGroupDisabled(Identity group)
        {
            string groupKey = ConversionHelper.ConvertIdentityToString(group);

            return this.DisabledGroups.Contains(groupKey);
        }

        public bool IsQuestionDisabled(Identity question)
        {
            string questionKey = ConversionHelper.ConvertIdentityToString(question);

            return this.DisabledQuestions.Contains(questionKey);
        }

        public bool IsVariableDisabled(Identity variable)
        {
            return this.DisabledVariables.Contains(variable);
        }

        public bool WasQuestionAnswered(Identity question)
        {
            string questionKey = ConversionHelper.ConvertIdentityToString(question);

            return this.AnsweredQuestions.Contains(questionKey);
        }

        public object GetAnswerSupportedInExpressions(Identity question)
        {
            string questionKey = ConversionHelper.ConvertIdentityToString(question);

            return this.AnswersSupportedInExpressions.ContainsKey(questionKey)
                ? this.AnswersSupportedInExpressions[questionKey]
                : null;
        }

        public Tuple<decimal, string>[] GetTextListAnswer(Identity question)
        {
            string questionKey = ConversionHelper.ConvertIdentityToString(question);

            return this.TextListAnswers.ContainsKey(questionKey)
                ? this.TextListAnswers[questionKey]
                : null;
        }

        public ReadOnlyCollection<decimal> GetRosterInstanceIds(Guid groupId, RosterVector outerRosterVector)
        {
            string groupKey = ConversionHelper.ConvertIdAndRosterVectorToString(groupId, outerRosterVector);

            return this.RosterGroupInstanceIds.ContainsKey(groupKey)
                ? this.RosterGroupInstanceIds[groupKey].ToReadOnlyCollection()
                : Enumerable.Empty<decimal>().ToReadOnlyCollection();
        }

        public string GetRosterTitle(Guid rosterId, RosterVector rosterVector)
        {
            string groupKey = ConversionHelper.ConvertIdAndRosterVectorToString(rosterId, rosterVector);

            return this.RosterTitles.ContainsKey(groupKey)
                ? this.RosterTitles[groupKey]
                : null;
        }

        public IEnumerable<Tuple<Identity, RosterVector>> GetAllLinkedToQuestionSingleOptionAnswers(IQuestionnaire questionnaire)
        {
            // we currently have a bug that after restore linked single option answers list is filled with not linked answers after sync
            return this.LinkedSingleOptionAnswersBuggy.Values.Where(answer => questionnaire.IsQuestionLinked(answer.Item1.Id) && !questionnaire.IsQuestionLinkedToRoster(answer.Item1.Id));
        }

        public IEnumerable<Tuple<Identity, RosterVector>> GetAllLinkedToRosterSingleOptionAnswers(
            IQuestionnaire questionnaire)
        {
            return this.LinkedSingleOptionAnswersBuggy.Values.Where(answer => questionnaire.IsQuestionLinkedToRoster(answer.Item1.Id));
        }

        public IEnumerable<Tuple<Identity, RosterVector>> GetAllAnswersOnSingleOptionLinkedQuestions(IQuestionnaire questionnaire)
        {
            return this.LinkedSingleOptionAnswersBuggy.Values.Where(answer => questionnaire.IsQuestionLinked(answer.Item1.Id) || questionnaire.IsQuestionLinkedToRoster(answer.Item1.Id));
        }

        public IEnumerable<Tuple<Identity, RosterVector[]>> GetAllLinkedMultipleOptionsAnswers(IQuestionnaire questionnaire)
        {
            return this.LinkedMultipleOptionsAnswers.Values.Where(answer => !questionnaire.IsQuestionLinkedToRoster(answer.Item1.Id));
        }

        public IEnumerable<Tuple<Identity, RosterVector[]>> GetAllLinkedToRosterMultipleOptionsAnswers(IQuestionnaire questionnaire)
        {
            return this.LinkedMultipleOptionsAnswers.Values.Where(answer => questionnaire.IsQuestionLinkedToRoster(answer.Item1.Id));
        }

        public IReadOnlyCollection<RosterVector> GetOptionsForLinkedQuestion(Identity linkedQuestionIdentity)
            => this.LinkedQuestionOptions.GetOrNull(linkedQuestionIdentity);

        public object GetAnswer(Identity identity)
        {
            string questionKey = ConversionHelper.ConvertIdentityToString(identity);

            if (this.AnswersSupportedInExpressions.ContainsKey(questionKey))
                return this.AnswersSupportedInExpressions[questionKey];

            if (this.TextListAnswers.ContainsKey(questionKey))
                return this.TextListAnswers[questionKey];

            if (LinkedMultipleOptionsAnswers.ContainsKey(questionKey))
                return this.LinkedMultipleOptionsAnswers[questionKey].Item2;

            return null;
        }

        public void ChangeVariables(ChangedVariable[] changedVariables)
        {
            changedVariables.ForEach(variable => this.VariableValues[variable.Identity] = variable.NewValue);
        }

        public void EnableVariables(Identity[] variables)
        {
            variables.ForEach(x => this.DisabledVariables.Remove(x));
        }

        public void DisableVariables(Identity[] variables)
        {
            variables.ForEach(x => this.DisabledVariables.Add(x));
        }
    }
}