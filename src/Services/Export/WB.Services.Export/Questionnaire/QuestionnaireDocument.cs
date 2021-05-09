using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Caching.Memory;
using WB.Services.Export.Interview;
using WB.Services.Export.InterviewDataStorage.InterviewDataExport;
using WB.ServicesIntegration.Export;

namespace WB.Services.Export.Questionnaire
{
    public class QuestionnaireDocument : Group
    {
        public QuestionnaireDocument(List<IQuestionnaireEntity>? children = null) : base(children)
        {
            this.memoryCache = new MemoryCache(new MemoryCacheOptions());
            this.Translations = new List<Translation>();
            this.Categories = new List<Categories>();
        }

        public List<Translation> Translations { get; set; }

        public List<Categories> Categories { get; set; }

        public string Id { get; set;} = String.Empty;
        public int Revision { get; set; }

        public bool IsIntegerQuestion(Guid publicKey)
        {
            var result = this.Find<NumericQuestion>(x => x.PublicKey == publicKey && x.QuestionType == QuestionType.Numeric && x.IsInteger).FirstOrDefault();
            return result != null;
        }

        public string? GetQuestionVariableName(Guid publicKey)
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
            Group? group = this.Find<Group>(groupId);

            if (group == null) return false;

            return group.IsRoster;
        }

        private IEnumerable<Guid> GetAllParentGroupsForEntityStartingFromBottom(Guid entityId)
        {
            IQuestionnaireEntity? entity = this.Find<IQuestionnaireEntity>(entityId);

            if(entity == null)
                throw new InvalidOperationException($"Entity {entityId} was not found.");
            
            var parentGroup = entity.GetParent() as Group;
            if (parentGroup == null)
                throw new InvalidOperationException($"Parent for entity {entityId} is not valid.");

            return this.GetSpecifiedGroupAndAllItsParentGroupsStartingFromBottom(parentGroup);
        }

        private IEnumerable<Guid> GetSpecifiedGroupAndAllItsParentGroupsStartingFromBottom(Group group)
        {
            return this.GetSpecifiedGroupAndAllItsParentGroupsStartingFromBottom(group, this).Select(_ => _.PublicKey);
        }

        private IEnumerable<Group> GetSpecifiedGroupAndAllItsParentGroupsStartingFromBottom(Group group, QuestionnaireDocument document)
        {
            Group? groupIterator = group;

            while (groupIterator != null && groupIterator != document)
            {
                yield return groupIterator;
                groupIterator = groupIterator.GetParent() as Group;
            }
        }

        public string? GetRosterVariableName(Guid publicKey)
        {
            Group? group = this.Find<Group>(publicKey);
            return group?.VariableName;
        }

        public string GetValidationMessage(Guid publicKey, int conditionIndex)
        {
            var entity = this.Find<IValidatableQuestionnaireEntity>(publicKey);
            if (entity == null) return string.Empty;
            
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
                Group? roster = this.Find<Group>(rosterId);
                if (roster == null) throw new InvalidOperationException("Roster was not found.");

                return roster.RosterSizeQuestionId ?? roster.PublicKey;
            });
        }

        public Guid[] GetAllGroups()
            => this.GroupsCache.Values.Select(question => question.PublicKey).ToArray();

        public Group GetGroup(Guid groupId) => this.GroupsCache[groupId];

        public T? Find<T>(Guid publicKey) where T : class, IQuestionnaireEntity
        {
            return EntitiesCache[publicKey] as T; ;
        }

        private Dictionary<ValueVector<Guid>, Guid[]>? rostersInLevelCache = null;

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

        private readonly MemoryCache memoryCache;

        private Dictionary<Guid, IQuestionnaireEntity>? entitiesCache;
        private Dictionary<Guid, IQuestionnaireEntity> EntitiesCache
        {
            get
            {
                return this.entitiesCache ??= this.Find<IQuestionnaireEntity>(_ => true)
                    .ToDictionary(
                        entity => entity.PublicKey,
                        entity => entity);
            }
        }

        private Dictionary<Guid, Group>? groupsCache; 
        private Dictionary<Guid, Group> GroupsCache
        {
            get
            {
                return this.groupsCache ??= EntitiesCache.Where(kv => kv.Value is Group)
                    .ToDictionary(
                        group => @group.Key,
                        group => (Group)@group.Value);
            }
        }

        public QuestionnaireIdentity QuestionnaireId { get; set; } = null!;

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

                if (current.DoesSupportDataTable || current.DoesSupportEnablementTable || current.DoesSupportValidityTable)
                {
                    yield return current;
                }
            }
        }

        private QuestionnaireDatabaseStructure? databaseStructure;
        public QuestionnaireDatabaseStructure DatabaseStructure =>
            databaseStructure ??= new QuestionnaireDatabaseStructure(this);

        public bool IsDeleted { get; set; }

        public string? DefaultLanguageName { get; set; }
    }
}
