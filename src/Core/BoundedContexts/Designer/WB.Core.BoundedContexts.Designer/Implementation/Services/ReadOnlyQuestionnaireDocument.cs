using System;
using System.Collections.Generic;
using System.Linq;
using Main.Core.Documents;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using WB.Core.BoundedContexts.Designer.ValueObjects;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.SharedKernels.SurveySolutions.Documents;

namespace WB.Core.BoundedContexts.Designer.Implementation.Services
{
    public class ReadOnlyQuestionnaireDocument
    {
        public class QuestionnaireItemTypeReference
        {
            public Guid Id { get; }
            public Type Type { get; }

            public QuestionnaireItemTypeReference(Guid id, Type type)
            {
                this.Id = id;
                this.Type = type;
            }
        }

        private class EntityWithMeta
        {
            public EntityWithMeta(IComposite entity, EntityMeta meta = null)
            {
                Entity = entity;
                Meta = meta ?? new EntityMeta();
            }

            public IComposite Entity { get; private set; }
            public EntityMeta Meta { get; private set; }
        }

        private class EntityMeta
        {
            public RosterScope RosterScope { get; set; }
        }

        private readonly List<EntityWithMeta> allItems;
        private IEnumerable<IComposite> entities => this.allItems.Select(x => x.Entity);

        public QuestionnaireDocument Questionnaire { get; private set; }
        public string Translation { get; private set; }

        public ReadOnlyQuestionnaireDocument(QuestionnaireDocument questionnaire, string translation = null)
        {
            this.Translation = translation;
            this.Questionnaire = questionnaire;

            this.allItems = this.CreateEntitiesIdAndTypePairsInQuestionnaireFlowOrder(this.Questionnaire);
        }

        public Dictionary<Guid, Macro> Macros => this.Questionnaire.Macros;
        public Dictionary<Guid, LookupTable> LookupTables => this.Questionnaire.LookupTables;
        public List<Attachment> Attachments => this.Questionnaire.Attachments;
        public List<Translation> Translations => this.Questionnaire.Translations;
        public string Title => this.Questionnaire.Title;
        public Guid PublicKey => this.Questionnaire.PublicKey;

        public T Find<T>(Guid publicKey) where T : class, IComposite
        {
            return this.FindEntityWithMeta(publicKey)?.Entity as T;
        }

        private EntityWithMeta FindEntityWithMeta(Guid publicKey)
        {
            return this.allItems.FirstOrDefault(x => x.Entity.PublicKey == publicKey);
        }

        public IEnumerable<T> Find<T>() where T : class
            => this.entities.Where(x => x is T).Select(x => x).Cast<T>();

        public IEnumerable<T> Find<T>(Func<T, bool> condition) where T : class
            => this.Find<T>().Where(condition);

        public T FirstOrDefault<T>(Func<T, bool> condition) where T : class
            => this.Find(condition).FirstOrDefault();

        public IEnumerable<QuestionnaireItemTypeReference> GetAllEntitiesIdAndTypePairsInQuestionnaireFlowOrder()
        {
            return this.entities.Select(x => new QuestionnaireItemTypeReference(x.PublicKey, x.GetType()));
        }

        private List<EntityWithMeta> CreateEntitiesIdAndTypePairsInQuestionnaireFlowOrder(QuestionnaireDocument questionnaire)
        {
            var result = new List<EntityWithMeta>();
            var stack = new Stack<IComposite>();
            stack.Push(questionnaire);
            while (stack.Any())
            {
                var current = stack.Pop();
                for (int i = current.Children.Count - 1; i >= 0; i--)
                {
                    var child = current.Children[i];
                    stack.Push(child);
                }
                if (!(current is QuestionnaireDocument))
                    result.Add(new EntityWithMeta(current, BuildEntityMeta(current)));
            }
            return result;
        }

        private EntityMeta BuildEntityMeta(IComposite entity)
        {
            return new EntityMeta
            {
                RosterScope = CalculateRosterScopeIds(entity)
            };
        }

        public IEnumerable<T> FindInGroup<T>(Guid groupId)
        {
            var group = Find<IGroup>(groupId);
            return group.TreeToEnumerable<IComposite>(x => x.Children).Where(x => x.PublicKey!=groupId && x is T).Cast<T>();
        }

        public Guid[] GetParentGroupsIds(IComposite entity)
        {
            List<IGroup> parents = new List<IGroup>();
            var parent = (IGroup)entity.GetParent();
            while (parent != null && !(parent is QuestionnaireDocument))
            {
                parents.Add(parent);
                parent = (IGroup)parent.GetParent();
            }
            return parents.Select(x => x.PublicKey).ToArray();
        }

        public IEnumerable<IComposite> FindEntitiesFromTheSameOrUpperRosterScope(IComposite entity)
        {
            var entityRosterScope = this.GetRosterScope(entity);
            return this.allItems
                .Where(x => entityRosterScope.Equals(x.Meta.RosterScope) || x.Meta.RosterScope.IsParentScopeFor(entityRosterScope))
                .Select(x => x.Entity);
        }

        public RosterScope GetRosterScope(IComposite entity)
        {
            return this.GetRosterScope(entity.PublicKey);
        }

        public RosterScope GetRosterScope(Guid entityId)
        {
            return FindEntityWithMeta(entityId).Meta.RosterScope;
        }

        private static RosterScope CalculateRosterScopeIds(IComposite questionnaireEntity)
        {
            return new RosterScope(GetAllParentsFromBottomToTop(questionnaireEntity)
                .Where(x => x.IsRoster)
                .Select(x => x.RosterSizeSource == RosterSizeSourceType.FixedTitles ? x.PublicKey : (x.RosterSizeQuestionId ?? Guid.Empty))
                .Reverse());
        }

        private static IEnumerable<IGroup> GetAllParentsFromBottomToTop(IComposite questionnaireEntity)
        {
            IComposite entity = questionnaireEntity;
            while (entity != null && !(entity is QuestionnaireDocument))
            {
                IGroup group = entity as IGroup;
                if (group!=null)
                {
                    yield return group;
                }

                entity = (IGroup)entity.GetParent();
            }
        }

        public bool IsQuestionIsNumeric(Guid questionId)
        {
            return Find<IQuestion>(questionId)?.QuestionType == QuestionType.Numeric;
        }

        public IGroup GetRoster(Guid rosterId)
        {
            var roster = Find<IGroup>(rosterId);
            if (roster?.IsRoster ?? false)
                return roster;
            return null;
        }

        public string GetRosterSourceType(IGroup roster, ReadOnlyQuestionnaireDocument document)
        {
            return roster.RosterSizeQuestionId.HasValue
                ? Find<IQuestion>(roster.RosterSizeQuestionId.Value)?.QuestionType.ToString().ToLower()
                : null;
        }

        public bool IsRoster(IComposite entity)
        {
            return (entity as Group)?.IsRoster ?? false;
        }
    }
}