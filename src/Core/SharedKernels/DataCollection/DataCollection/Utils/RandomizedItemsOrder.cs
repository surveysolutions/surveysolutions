using System;
using System.Collections.Generic;
using System.Linq;
using WB.Core.SharedKernels.DataCollection.Aggregates;

namespace WB.Core.SharedKernels.DataCollection.Utils
{
    /// <summary>
    /// Deterministic display order of questionnaire items marked with "Randomize order".
    /// 
    /// Order is calculated from interview random value (Quest.IRnd()), questionnaire structure,
    /// questionnaire item ids and roster instance vectors only, so it is reproducible on every platform.
    /// 
    /// Do not change the implementation: it has to stay the same for already existing interviews 
    /// and it is duplicated in export service (WB.Services.Export.Interview.RandomizedItemsOrder).
    /// </summary>
    public static class RandomizedItemsOrder
    {
        private const ulong AlgorithmVersion = 1;
        private const ulong ItemsDomain = 1;
        private const ulong RosterInstancesDomain = 2;
        private const ulong AnswerOptionsDomain = 3;
        private const ulong LinkedAnswerOptionsDomain = 4;
        private const ulong Gamma = 0x9E3779B97F4A7C15UL;

        public static ulong GetItemSortKey(double seed, Guid randomizationGroupId, RosterVector parentRosterVector, Guid itemId)
        {
            var hash = Start(seed, ItemsDomain);
            hash = Combine(hash, randomizationGroupId);
            hash = Combine(hash, parentRosterVector);
            hash = Combine(hash, itemId);
            return hash;
        }

        public static ulong GetRosterInstanceSortKey(double seed, Guid rosterId, RosterVector rosterInstanceVector)
        {
            var hash = Start(seed, RosterInstancesDomain);
            hash = Combine(hash, rosterId);
            hash = Combine(hash, rosterInstanceVector);
            return hash;
        }

        public static ulong GetAnswerOptionSortKey(double seed, Guid questionId, RosterVector questionRosterVector, int optionValue)
        {
            var hash = Start(seed, AnswerOptionsDomain);
            hash = Combine(hash, questionId);
            hash = Combine(hash, questionRosterVector);
            hash = Combine(hash, unchecked((ulong)(uint)optionValue));
            return hash;
        }

        public static ulong GetLinkedAnswerOptionSortKey(double seed, Guid questionId, RosterVector questionRosterVector, RosterVector linkedOption)
        {
            var hash = Start(seed, LinkedAnswerOptionsDomain);
            hash = Combine(hash, questionId);
            hash = Combine(hash, questionRosterVector);
            hash = Combine(hash, linkedOption);
            return hash;
        }

        public static double GetSeed(Guid interviewId) => new InterviewProperties(interviewId).IRnd();

        /// <summary>
        /// Sorts available answer options of categorical question in display order if question has "Randomize answer options" flag.
        /// Order of each option depends only on interview, question identity (including roster vector) and option code,
        /// so adding or removing (filtering) options does not change relative order of remaining options.
        /// </summary>
        public static IEnumerable<T> OrderAnswerOptionsForDisplay<T>(IQuestionnaire questionnaire, double seed, Identity questionIdentity,
            IEnumerable<T> options, Func<T, int> getOptionValue)
        {
            if (questionnaire == null || questionIdentity == null || !questionnaire.ShouldRandomizeAnswerOptions(questionIdentity.Id))
                return options;

            var context = questionIdentity.RosterVector ?? RosterVector.Empty;
            return options
                .Select(option => (option, value: getOptionValue(option)))
                .Select(x => (x.option, x.value, key: GetAnswerOptionSortKey(seed, questionIdentity.Id, context, x.value)))
                .OrderBy(x => x.key)
                .ThenBy(x => x.value)
                .Select(x => x.option)
                .ToList();
        }

        /// <summary>
        /// Sorts available answer options of question linked to roster or question in roster in display order 
        /// if question has "Randomize answer options" flag. Stable identity of linked option (roster vector) is used.
        /// </summary>
        public static IEnumerable<T> OrderLinkedAnswerOptionsForDisplay<T>(IQuestionnaire questionnaire, double seed, Identity questionIdentity,
            IEnumerable<T> options, Func<T, RosterVector> getLinkedOption)
        {
            if (questionnaire == null || questionIdentity == null || !questionnaire.ShouldRandomizeAnswerOptions(questionIdentity.Id))
                return options;

            var context = questionIdentity.RosterVector ?? RosterVector.Empty;
            return options
                .Select(option => (option, vector: getLinkedOption(option) ?? RosterVector.Empty))
                .Select(x => (x.option, x.vector, key: GetLinkedAnswerOptionSortKey(seed, questionIdentity.Id, context, x.vector)))
                .OrderBy(x => x.key)
                .ThenBy(x => x.vector, RosterVectorComparer)
                .Select(x => x.option)
                .ToList();
        }

        private static readonly IComparer<RosterVector> RosterVectorComparer = Comparer<RosterVector>.Create((x, y) =>
        {
            var left = x.Array;
            var right = y.Array;
            for (int i = 0; i < Math.Min(left.Length, right.Length); i++)
            {
                var result = left[i].CompareTo(right[i]);
                if (result != 0) return result;
            }

            return left.Length.CompareTo(right.Length);
        });

        /// <summary>
        /// Returns all child entity ids of <paramref name="parentId"/> (questionnaire root, section, sub-section or roster)
        /// in the order they should be displayed. Randomization groups are built from questionnaire structure only:
        /// consecutive siblings with "Randomize order" flag form one group, any other sibling breaks it.
        /// </summary>
        public static IReadOnlyList<Guid> GetChildrenOrder(IQuestionnaire questionnaire, double seed, Guid parentId, RosterVector parentRosterVector)
        {
            var children = questionnaire.GetChildEntityIds(parentId).ToArray();
            if (!questionnaire.HasRandomizedChildren(parentId))
                return children;

            var context = parentRosterVector ?? RosterVector.Empty;
            var result = new Guid[children.Length];

            int index = 0;
            while (index < children.Length)
            {
                if (!questionnaire.IsOrderRandomized(children[index]))
                {
                    result[index] = children[index];
                    index++;
                    continue;
                }

                int groupStart = index;
                while (index < children.Length && questionnaire.IsOrderRandomized(children[index]))
                    index++;

                var groupId = children[groupStart];
                var ordered = children
                    .Skip(groupStart)
                    .Take(index - groupStart)
                    .Select((id, position) => new { id, position, key = GetItemSortKey(seed, groupId, context, id) })
                    .OrderBy(x => x.key)
                    .ThenBy(x => x.position)
                    .Select(x => x.id)
                    .ToArray();

                Array.Copy(ordered, 0, result, groupStart, ordered.Length);
            }

            return result;
        }

        /// <summary>
        /// Sorts runtime children of <paramref name="parentIdentity"/> in display order. 
        /// Children may be filtered (by visibility, type etc.), filtering does not affect the order of remaining items.
        /// Roster instances are kept together as single block and if roster is randomized, instances are shuffled within the block.
        /// </summary>
        /// <param name="parentIdentity">Parent group identity or <c>null</c> for questionnaire root (sections)</param>
        public static IEnumerable<Identity> OrderForDisplay(IQuestionnaire questionnaire, double seed, Identity parentIdentity, IEnumerable<Identity> children)
        {
            var parentId = parentIdentity?.Id ?? questionnaire.QuestionnaireId;
            if (!questionnaire.HasRandomizedChildren(parentId))
                return children;

            var parentRosterVector = parentIdentity?.RosterVector ?? RosterVector.Empty;
            var positions = new Dictionary<Guid, int>();
            var order = GetChildrenOrder(questionnaire, seed, parentId, parentRosterVector);
            for (int i = 0; i < order.Count; i++)
                positions[order[i]] = i;

            return children
                .Select((identity, index) => new
                {
                    identity,
                    index,
                    position = positions.TryGetValue(identity.Id, out var position) ? position : int.MaxValue,
                    instanceKey = questionnaire.IsRosterGroup(identity.Id) && questionnaire.IsOrderRandomized(identity.Id)
                        ? GetRosterInstanceSortKey(seed, identity.Id, identity.RosterVector)
                        : 0UL
                })
                .OrderBy(x => x.position)
                .ThenBy(x => x.instanceKey)
                .ThenBy(x => x.index)
                .Select(x => x.identity)
                .ToList();
        }

        private static ulong Start(double seed, ulong domain)
        {
            var hash = Mix(unchecked((ulong)BitConverter.DoubleToInt64Bits(seed)) ^ AlgorithmVersion);
            return Combine(hash, domain);
        }

        private static ulong Combine(ulong hash, RosterVector rosterVector)
        {
            var coordinates = rosterVector?.Array ?? Array.Empty<int>();
            hash = Combine(hash, (ulong)coordinates.Length);
            foreach (var coordinate in coordinates)
                hash = Combine(hash, unchecked((ulong)(uint)coordinate));
            return hash;
        }

        private static ulong Combine(ulong hash, Guid value)
        {
            var bytes = value.ToByteArray();
            hash = Combine(hash, ToUInt64(bytes, 0));
            hash = Combine(hash, ToUInt64(bytes, 8));
            return hash;
        }

        private static ulong Combine(ulong hash, ulong value) => Mix(unchecked(hash + Gamma) ^ value);

        private static ulong ToUInt64(byte[] bytes, int offset)
        {
            ulong result = 0;
            for (int i = 7; i >= 0; i--)
                result = (result << 8) | bytes[offset + i];
            return result;
        }

        // SplitMix64 finalizer
        private static ulong Mix(ulong z)
        {
            unchecked
            {
                z = (z ^ (z >> 30)) * 0xBF58476D1CE4E5B9UL;
                z = (z ^ (z >> 27)) * 0x94D049BB133111EBUL;
                return z ^ (z >> 31);
            }
        }
    }
}
