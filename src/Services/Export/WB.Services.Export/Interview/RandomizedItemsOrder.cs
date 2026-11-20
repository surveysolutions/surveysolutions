using System;
using System.Collections.Generic;
using System.Linq;
using WB.Services.Export.Questionnaire;
using WB.Services.Infrastructure.EventSourcing;

namespace WB.Services.Export.Interview
{
    /// <summary>
    /// Deterministic display order of questionnaire items marked with "Randomize order".
    /// 
    /// Do not change the implementation: it has to produce exactly the same order as 
    /// WB.Core.SharedKernels.DataCollection.Utils.RandomizedItemsOrder used by interview applications.
    /// </summary>
    public static class RandomizedItemsOrder
    {
        private const ulong AlgorithmVersion = 1;
        private const ulong ItemsDomain = 1;
        private const ulong RosterInstancesDomain = 2;
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

        public static bool IsOrderRandomized(QuestionnaireDocument questionnaire, IQuestionnaireEntity entity)
        {
            var isRandomized = entity switch
            {
                Group group => group.RandomizeOrder,
                Question question => question.RandomizeOrder,
                StaticText staticText => staticText.RandomizeOrder,
                _ => false
            };

            if (!isRandomized || entity is QuestionnaireDocument)
                return false;

            return entity.PublicKey != questionnaire.CoverPageSectionId
                && entity.GetParent()?.PublicKey != questionnaire.CoverPageSectionId;
        }

        public static bool HasRandomizedChildren(QuestionnaireDocument questionnaire, Group group)
            => group.PublicKey != questionnaire.CoverPageSectionId
               && group.Children.Any(child => IsOrderRandomized(questionnaire, child));

        public static IReadOnlyList<IQuestionnaireEntity> GetChildrenOrder(QuestionnaireDocument questionnaire, double seed,
            Group parent, RosterVector parentRosterVector)
        {
            var children = parent.Children.ToArray();
            if (!HasRandomizedChildren(questionnaire, parent))
                return children;

            var result = new IQuestionnaireEntity[children.Length];

            int index = 0;
            while (index < children.Length)
            {
                if (!IsOrderRandomized(questionnaire, children[index]))
                {
                    result[index] = children[index];
                    index++;
                    continue;
                }

                int groupStart = index;
                while (index < children.Length && IsOrderRandomized(questionnaire, children[index]))
                    index++;

                var groupId = children[groupStart].PublicKey;
                var ordered = children
                    .Skip(groupStart)
                    .Take(index - groupStart)
                    .Select((entity, position) => new { entity, position, key = GetItemSortKey(seed, groupId, parentRosterVector, entity.PublicKey) })
                    .OrderBy(x => x.key)
                    .ThenBy(x => x.position)
                    .Select(x => x.entity)
                    .ToArray();

                Array.Copy(ordered, 0, result, groupStart, ordered.Length);
            }

            return result;
        }

        private static ulong Start(double seed, ulong domain)
        {
            var hash = Mix(unchecked((ulong)BitConverter.DoubleToInt64Bits(seed)) ^ AlgorithmVersion);
            return Combine(hash, domain);
        }

        private static ulong Combine(ulong hash, RosterVector rosterVector)
        {
            var coordinates = rosterVector?.Coordinates ?? (IReadOnlyCollection<int>)Array.Empty<int>();
            hash = Combine(hash, (ulong)coordinates.Count);
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
