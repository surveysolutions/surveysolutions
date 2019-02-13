using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Caching.Memory;
using WB.Services.Export.Interview;
using WB.Services.Infrastructure;

namespace WB.Services.Export.Questionnaire
{
    public class QuestionnaireDocument : Group
    {
        public QuestionnaireDocument(List<IQuestionnaireEntity> children = null) : base(children)
        {
            this.memoryCache = new MemoryCache(new MemoryCacheOptions());
        }

        public string Id { get; set;}

        public bool IsIntegerQuestion(Guid publicKey)
        {
            var result = this.Find<NumericQuestion>(x => x.PublicKey == publicKey && x.QuestionType == QuestionType.Numeric && x.IsInteger).FirstOrDefault();
            return result != null;
        }

        public string GetQuestionVariableName(Guid publicKey)
        {
            var result = this.FirstOrDefault<Question>(x => x.PublicKey == publicKey);
            return result?.VariableName;
        }

        public override bool IsRoster => false;

        public IEnumerable<Guid> GetRostersFromTopToSpecifiedEntity(Guid entityId)
        {
            return this
                .GetAllParentGroupsForEntityStartingFromBottom(entityId)
                .Where(this.IsRosterGroup)
                .Reverse()
                .ToList();
        }

        private bool IsRosterGroup(Guid groupId)
        {
            Group group = this.Find<Group>(groupId);

            if (group == null) return false;

            return group.IsRoster;
        }

        private IEnumerable<Guid> GetAllParentGroupsForEntityStartingFromBottom(Guid entityId)
        {
            IQuestionnaireEntity entity = this.Find<IQuestionnaireEntity>(entityId);

            var parentGroup = (Group)entity.GetParent();

            return this.GetSpecifiedGroupAndAllItsParentGroupsStartingFromBottom(parentGroup);
        }

        private IEnumerable<Guid> GetSpecifiedGroupAndAllItsParentGroupsStartingFromBottom(Group group)
        {
            return this.GetSpecifiedGroupAndAllItsParentGroupsStartingFromBottom(group, this).Select(_ => _.PublicKey);
        }

        private IEnumerable<Group> GetSpecifiedGroupAndAllItsParentGroupsStartingFromBottom(Group group, QuestionnaireDocument document)
        {
            while (group != document)
            {
                yield return group;
                group = (Group)group.GetParent();
            }
        }

        public string GetRosterVariableName(Guid publicKey)
        {
            Group group = this.Find<Group>(publicKey);
            return group?.VariableName;
        }

        public string GetValidationMessage(Guid publicKey, int conditionIndex)
        {
            var entity = this.Find<IValidatableQuestionnaireEntity>(publicKey);
            return entity.ValidationConditions[conditionIndex].Message;
        }

        public Guid[] GetRosterSizeSourcesForEntity(Guid entityId)
        {
            return this.memoryCache.GetOrCreate("RosterSizeSource:" + entityId, entry =>
            {
                var entity = this.Find<IQuestionnaireEntity>(entityId);

                var rosterSizes = new List<Guid>();

                while (entity != this && entity != null)
                {
                    if (entity is Group group)
                    {
                        if (group.IsRoster)
                            rosterSizes.Insert(0, this.GetRosterSource(group.PublicKey));
                    }

                    entity = entity.GetParent();
                }

                return rosterSizes.ToArray();
            });
        }

        private Guid GetRosterSource(Guid rosterId)
        {
            return this.memoryCache.GetOrCreate("GetRosterSource:" + rosterId, entry =>
            {
                Group roster = this.Find<Group>(rosterId);

                return roster.RosterSizeQuestionId ?? roster.PublicKey;
            });
        }

        public Guid[] GetAllGroups()
            => this.GroupsCache.Values.Select(question => question.PublicKey).ToArray();

        public Group GetGroup(Guid groupId) => this.GroupsCache[groupId];

        private Dictionary<ValueVector<Guid>, Guid[]> rostersInLevelCache = null;

        public Guid[] GetRostersInLevel(ValueVector<Guid> rosterScope)
        {
            return this.memoryCache.GetOrCreate("getrostersinlevel:" + rosterScope.ToString(), entry =>
            {
                if (rostersInLevelCache == null)
                {
                    rostersInLevelCache = GetAllGroups()
                        .Select(x => new
                        {
                            RosterScope = new ValueVector<Guid>(GetRosterSizeSourcesForEntity(x)),
                            RosterId = x
                        })
                        .GroupBy(x => x.RosterScope)
                        .ToDictionary(x => x.Key, x => x.Select(e => e.RosterId).ToArray());
                }

                return rostersInLevelCache[rosterScope];
            });
        }

        private Dictionary<Guid, Group> groupsCache;
        private MemoryCache memoryCache;

        private Dictionary<Guid, Group> GroupsCache
        {
            get
            {
                return this.groupsCache ?? (
                           this.groupsCache = this.Find<Group>(_ => true)
                               .ToDictionary(
                                   group => group.PublicKey,
                                   group => group));
            }
        }

        public QuestionnaireId QuestionnaireId { get; set; }

        public IEnumerable<Group> GetAllStoredGroups()
        {
            Queue<Group> queue = new Queue<Group>(this.Children.OfType<Group>());

            while (queue.Count > 0)
            {
                var current = queue.Dequeue();

                foreach (var subChild in current.Children.OfType<Group>())
                {
                    queue.Enqueue(subChild);
                }

                if (current.IsRoster || current.HasAnyExportableQuestions)
                {
                    yield return current;
                }
            }
        }

        private string tableName;
        public override string TableName
        {
            get
            {
                if (tableName != null) return tableName;
                tableName = $"{CompressQuestionnaireId(this.QuestionnaireId)}_{this.VariableName ?? this.PublicKey.FormatGuid()}";
                return tableName;
            }
        }

        public IEnumerable<Group> GetInterviewLevelGroupsWithQuestionOrVariables()
        {
            var itemsQueue = new Queue<Group>();
            itemsQueue.Enqueue(this);

            while (itemsQueue.Count > 0)
            {
                var currentGroup = itemsQueue.Dequeue();

                if (currentGroup.Children.Any(e => e is Question || e is Variable))
                    yield return currentGroup;

                var childGroups = currentGroup.Children
                    .Where(g => g is Group childGroup && !childGroup.IsRoster).Cast<Group>();

                foreach (var childItem in childGroups)
                {
                    itemsQueue.Enqueue(childItem);
                }
            }
        }
    }
}
