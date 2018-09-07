using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.SharedKernels.Questionnaire.Documents;
using WB.Core.SharedKernels.SurveySolutions.Documents;

namespace Main.Core.Documents
{
    public class QuestionnaireDocument : IQuestionnaireDocument, IView
    {
        //is used for deserrialization
        public QuestionnaireDocument(List<IComposite> children = null)
        {
            this.CreationDate = DateTime.Now;
            this.LastEntryDate = DateTime.Now;
            this.PublicKey = Guid.NewGuid();
            this.Id = this.PublicKey.FormatGuid();
            this.ConditionExpression = string.Empty;
            this.IsPublic = false;
            this.Macros = new Dictionary<Guid, Macro>();
            this.LookupTables = new Dictionary<Guid, LookupTable>();
            this.Attachments = new List<Attachment>();
            this.Translations = new List<Translation>();

            if(children == null)
                this.children = new List<IComposite>();
            else
            {
                this.children = children;
                ConnectChildrenWithParentIfNeeded();
            }
        }
        
        public string Id { get; set; }

        private List<IComposite> children;

        public ReadOnlyCollection<IComposite> Children
        {
            get
            {
                return new ReadOnlyCollection<IComposite>(this.children);
            }
            set
            {
                children = new List<IComposite>(value);
                ConnectChildrenWithParentIfNeeded();
            } 
        }

        public Dictionary<Guid, Macro> Macros { get; set; }

        public Dictionary<Guid, LookupTable> LookupTables { get; set; }

        public List<Attachment> Attachments { get; set; }

        public List<Translation> Translations { get; set; }

        public Guid? DefaultTranslation { get; set; }

        public DateTime? CloseDate { get; set; }

        public string ConditionExpression { get; set; }

        public bool HideIfDisabled { get; set; }

        public DateTime CreationDate { get; set; }

        public DateTime LastEntryDate { get; set; }

        public DateTime? OpenDate { get; set; }

        public bool IsDeleted { get; set; }

        public Guid? CreatedBy { get; set; }

        public bool IsPublic { get; set; }

        private IComposite parent;

        private bool childrenWereConnected = false;

        public IComposite GetParent()
        {
            return parent;
        }

        public void SetParent(IComposite parent)
        {
            this.parent = parent;
            this.childrenWereConnected = false;
        }

        public Guid PublicKey { get; set; }

        public string Title { get; set; }

        public string Description { get; set; }

        public QuestionnaireMetaInfo Metadata { get; set; }

        public string VariableName { get; set; }

        public bool IsRoster => false;

        public Guid? RosterSizeQuestionId => null;

        public RosterSizeSourceType RosterSizeSource => RosterSizeSourceType.Question;

        public string[] RosterFixedTitles { set {} }

        public FixedRosterTitle[] FixedRosterTitles => new FixedRosterTitle[0];

        public Guid? RosterTitleQuestionId => null;
        public void ReplaceChildEntityById(Guid id, IComposite newEntity)
        {
            int indexOfEntity = this.children.FindIndex(child => child.PublicKey == id);
            this.children[indexOfEntity] = newEntity;
            newEntity.SetParent(this);
            this.LastEntryDate = DateTime.UtcNow;
        }

        public long LastEventSequence { get; set; }

        public bool IsUsingExpressionStorage { get; set; }

        // fill in before export to HQ or Tester
        public List<Guid> ExpressionsPlayOrder { get; set; }

        public Dictionary<Guid, Guid[]> DependencyGraph { get; set; }
        public Dictionary<Guid, Guid[]> ValidationDependencyGraph { get; set; }

        public void Insert(int index, IComposite c, Guid? parentId)
        {
            if (!parentId.HasValue || this.PublicKey == parentId)
            {
                this.children.Insert(index, c);
                c.SetParent(this);
                c.ConnectChildrenWithParent();
                return;
            }

            var group = this.Find<Group>(parentId.Value);

            @group?.Insert(index, c, parentId);
            this.LastEntryDate = DateTime.UtcNow;
        }

        public void RemoveChild(Guid childId)
        {
            IComposite child = this.children.Find(c => c.PublicKey == childId);
            this.children.Remove(child);
            this.LastEntryDate = DateTime.UtcNow;
        }

        public void Add(IComposite c, Guid? parentKey)
        {
            if (!parentKey.HasValue || this.PublicKey == parentKey)
            {
                ////add to the root
                this.children.Add(c);
                c.SetParent(this);
            }
            else
            {
                var group = this.Find<Group>(parentKey.Value);
                if (@group != null)
                {
                    @group.Insert(Int32.MaxValue, c, parentKey);
                    c.SetParent(@group);
                }
            }
            
            c.ConnectChildrenWithParent();
            this.LastEntryDate = DateTime.UtcNow;
        }

        public void UpdateQuestion(Guid questionId, Action<IQuestion> update)
        {
            var question = this.Find<IQuestion>(questionId);

            if (question != null)
                update(question);
            this.LastEntryDate = DateTime.UtcNow;
        }

        public T Find<T>(Guid publicKey) where T : class, IComposite
        {
            foreach (IComposite child in this.Children)
            {
                if (child is T && child.PublicKey == publicKey)
                {
                    return child as T;
                }

                var subNodes = child.Find<T>(publicKey);
                if (subNodes != null)
                {
                    return subNodes;
                }
            }

            return null;
        }

        public IEnumerable<T> Find<T>() where T : class
            => this
                .children
                .TreeToEnumerable(composite => composite.Children)
                .Where(child => child is T)
                .Cast<T>();

        public IEnumerable<T> Find<T>(Func<T, bool> condition) where T : class
            => this
                .Find<T>()
                .Where(condition.Invoke);

        public T FirstOrDefault<T>(Func<T, bool> condition) where T : class
            => this.Find(condition).FirstOrDefault();

        public void ReplaceEntity(IComposite oldEntity, IComposite newEntity)
        {
            Guid oldEntityId = oldEntity.PublicKey;

            var entityParent = this.GetParentById(oldEntityId);
            entityParent?.ReplaceChildEntityById(oldEntityId, newEntity);
            this.LastEntryDate = DateTime.UtcNow;
        }

        public void UpdateGroup(Guid groupId, string title, string variableName, string description, string conditionExpression, bool hideIfDisabled)
        {
            this.UpdateGroup(groupId, group =>
            {
                @group.ConditionExpression = conditionExpression;
                @group.HideIfDisabled = hideIfDisabled;
                @group.Description = description;
                @group.VariableName = variableName;
                @group.Update(title);
            });
        }

        public void UpdateGroup(Guid groupId, Action<Group> update)
        {
            var group = this.Find<Group>(groupId);

            if (@group != null)
                update(@group);
            this.LastEntryDate = DateTime.UtcNow;
        }

        public void RemoveEntity(Guid entityId)
        {
            if (children.Any(child => child.PublicKey == entityId))
                this.RemoveChild(entityId);

            IComposite entityParent = this.GetParentById(entityId);

            entityParent?.RemoveChild(entityId);
            this.LastEntryDate = DateTime.UtcNow;
        }
        
        private IComposite GetParentOfItem(IComposite item)
        {
            if (ContainsChildItem(this, item))
                return this;

            return this
                .Find<IGroup>(group => ContainsChildItem(group, item))
                .SingleOrDefault();
        }

        public IGroup GetParentById(Guid entityId)
        {
            return this
                .Find<IGroup>(group => ContainsEntityWithSpecifiedId(group, entityId))
                .SingleOrDefault();
        }

        public IEnumerable<T> GetEntitiesByType<T>(IGroup startGroup = null) where T : class, IComposite
        {
            var result = new List<T>();
            var groups = new Queue<IComposite>();
            groups.Enqueue(startGroup ?? this);

            while (groups.Count != 0)
            {
                IComposite queueItem = groups.Dequeue();
                var entity = queueItem as T;
                if (entity != null)
                {
                    result.Add(entity);
                }

                foreach (IComposite child in queueItem.Children)
                {
                    groups.Enqueue(child);
                }
            }

            return result;
        }

        public IEnumerable<Group> GetAllGroups()
        {
            var result = new List<Group>();
            var groups = new Queue<IComposite>();
            groups.Enqueue(this);

            while (groups.Count != 0)
            {
                IComposite queueItem = groups.Dequeue();
                var @group = queueItem as Group;
                if (@group != null)
                {
                    result.Add(@group);
                }

                foreach (IComposite child in queueItem.Children)
                {
                    groups.Enqueue(child);
                }
            }

            return result;
        }

        public IComposite GetChapterOfItemById(Guid itemId)
        {
            IComposite item = this.GetItemOrLogWarning(itemId);
            IComposite parent = item.GetParent();

            while (!(parent is IQuestionnaireDocument) && parent != null)
            {
                item = parent;
                parent = parent.GetParent();
            }

            return item;
        }

        internal IEnumerable<IQuestion> GetAllQuestions()
        {
            var treeStack = new Stack<IComposite>();
            treeStack.Push(this);
            while (treeStack.Count > 0)
            {
                var node = treeStack.Pop();

                foreach (var child in node.Children)
                {
                    if (child is IGroup)
                    {
                        treeStack.Push(child);
                    }
                    else if (child is IQuestion)
                    {
                        yield return (child as IQuestion);
                    }
                }
            }
        }

        private static bool ContainsEntityWithSpecifiedId(IComposite container, Guid entityId)
        {
            return container.Children.Any(child => IsEntityWithSpecifiedId(child, entityId));
        }

        private static bool ContainsChildItem(IComposite container, IComposite item)
        {
            return container.Children.Any(child => child == item);
        }
        
        private static bool IsEntityWithSpecifiedId(IComposite entity, Guid entityId)
        {
            return entity.PublicKey == entityId;
        }

        private void ConnectChildrenWithParentIfNeeded()
        {
            if (childrenWereConnected)
                return;
            ConnectChildrenWithParent();
        }

        public void ConnectChildrenWithParent()
        {
            foreach (var item in this.children)
            {
                item.SetParent(this);
                item.ConnectChildrenWithParent();
            }

            this.childrenWereConnected = true;
        }

        public void ParseCategoricalQuestionOptions()
        {
            var questions = this.children.TreeToEnumerable(x => x.Children).OfType<IQuestion>();
            foreach (var question in questions)
            {
                bool isCategorical = question.QuestionType == QuestionType.SingleOption || question.QuestionType == QuestionType.MultyOption;

                if (isCategorical)
                {
                    ParseOptionsForCategoricalQuestion(question);
                }
            }
        }

        private static void ParseOptionsForCategoricalQuestion(IQuestion question)
        {
            foreach (var answer in question.Answers)
            {
                answer.AnswerCode = answer.GetParsedValue();
                if (!string.IsNullOrEmpty(answer.ParentValue))
                {
                    answer.ParentCode = decimal.Parse(answer.ParentValue, NumberStyles.Number, CultureInfo.InvariantCulture);
                }
            }
        }

        public void MoveItem(Guid itemId, Guid? targetGroupId, int targetIndex)
        {
            IComposite item = this.GetItemOrLogWarning(itemId);
            if (item == null)
                return;

            IComposite sourceContainer = this.GetParentOfItemOrLogWarning(item);
            if (sourceContainer == null)
                return;

            IComposite targetContainer = this.GetGroupOrRootOrLogWarning(targetGroupId);
            if (targetContainer == null)
                return;

            sourceContainer.RemoveChild(itemId);
            targetContainer.Insert(targetIndex, item, targetGroupId);
            this.LastEntryDate = DateTime.UtcNow;
        }

        private IComposite GetItemOrLogWarning(Guid itemId)
        {
            var itemToMove = this.Find<IComposite>(item => item.PublicKey == itemId).FirstOrDefault();

            return itemToMove;
        }

        private IComposite GetParentOfItemOrLogWarning(IComposite item)
        {
            IComposite foundParent = this.GetParentOfItem(item);

            return foundParent;
        }

        private IComposite GetGroupOrRootOrLogWarning(Guid? groupId)
        {
            if (groupId == null)
                return this;

            IComposite foundGroup = this.Find<IGroup>(group => group.PublicKey == groupId).FirstOrDefault();

            return foundGroup;
        }

        IComposite IComposite.Clone()
        {
            return this.Clone();
        }

        public QuestionnaireDocument Clone()
        {
            var doc = this.MemberwiseClone() as QuestionnaireDocument;

            doc.SetParent(null);

            var newChildren = new List<IComposite>();
            foreach (var composite in this.children)
            {
                newChildren.Add(composite.Clone());
            }

            doc.Children = newChildren.ToReadOnlyCollection();

            doc.Macros = new Dictionary<Guid, Macro>();
            this.Macros.ForEach(x => doc.Macros.Add(x.Key, x.Value.Clone()));

            doc.LookupTables = new Dictionary<Guid, LookupTable>();
            this.LookupTables.ForEach(x => doc.LookupTables.Add(x.Key, x.Value.Clone()));

            doc.Attachments = new List<Attachment>();
            this.Attachments.ForEach(x => doc.Attachments.Add(x.Clone()));

            doc.Translations = new List<Translation>();
            this.Translations.ForEach(x => doc.Translations.Add(x.Clone()));

            return doc;
        }
    }
}
