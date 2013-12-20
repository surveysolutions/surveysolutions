﻿using System;
using System.Collections.Generic;
using System.Linq;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using Main.Core.Entities.SubEntities.Question;
using Microsoft.Practices.ServiceLocation;
using WB.Core.GenericSubdomains.Logging;
using WB.Core.Infrastructure.ReadSide;

namespace Main.Core.Documents
{
    public class QuestionnaireDocument : IQuestionnaireDocument, IView
    {
        public QuestionnaireDocument()
        {
            this.logger = ServiceLocator.Current.GetInstance<ILogger>();
            this.CreationDate = DateTime.Now;
            this.LastEntryDate = DateTime.Now;
            this.PublicKey = Guid.NewGuid();
            this.Children = new List<IComposite>();
            this.ConditionExpression = string.Empty;
            this.IsPublic = false;
            this.SharedPersons = new List<Guid>();
        }


        public List<IComposite> Children { get; set; }

        public DateTime? CloseDate { get; set; }

        public string ConditionExpression { get; set; }

        public DateTime CreationDate { get; set; }

        public DateTime LastEntryDate { get; set; }

        public DateTime? OpenDate { get; set; }

        public bool IsDeleted { get; set; }

        public Guid? CreatedBy { get; set; }

        public bool IsPublic { get; set; }

        private IComposite parent;

        private ILogger logger;

        public Propagate Propagated
        {
            get
            {
                return Propagate.None;
            }

            set
            {
            }
        }

        public IComposite GetParent()
        {
            return parent;
        }

        public void SetParent(IComposite parent)
        {
            this.parent = parent;
        }

        public Guid PublicKey { get; set; }

        public string Title { get; set; }

        public string Description { get; set; }

        public bool IsRoster
        {
            get { return false; }
        }

        public Guid? RosterSizeQuestionId
        {
            get { return null; }
        }

        public RosterSizeSourceType RosterSizeSource
        {
            get { return RosterSizeSourceType.Question; }
        }

        public string[] RosterFixedTitles {
            get { return new string[0]; }
        }

        public Guid? RosterTitleQuestionId {
            get { return null; }
        }

        public List<Guid> SharedPersons { get; set; }

        public long LastEventSequence { get; set; }

        public void Insert(int index, IComposite c, Guid? parent)
        {
            if (!parent.HasValue || this.PublicKey == parent)
            {
                c.SetParent(this);
                this.Children.Insert(index, c);
                return;
            }

            var group = this.Find<Group>(parent.Value);
            if (@group != null)
            {
                @group.Children.Insert(index, c);
            }
        }

        public void Add(IComposite c, Guid? parent, Guid? parentPropagationKey)
        {
            if (!parent.HasValue || this.PublicKey == parent)
            {
                ////add to the root
                c.SetParent(this);
                this.Children.Add(c);
                return;
            }

            var group = this.Find<Group>(parent.Value);
            if (@group != null)
            {
                @group.Children.Add(c);
                return;
            }

            //// leave legacy for awhile
            throw new CompositeException();
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

        public IEnumerable<T> Find<T>(Func<T, bool> condition) where T : class
        {
            return
                this.Children.Where(a => a is T && condition(a as T)).Select(a => a as T).Union(
                    this.Children.SelectMany(q => q.Find(condition)));
        }

        public T FirstOrDefault<T>(Func<T, bool> condition) where T : class
        {
            return this.Children.Where(a => a is T && condition(a as T)).Select(a => a as T).FirstOrDefault()
                   ?? this.Children.SelectMany(q => q.Find(condition)).FirstOrDefault();
        }

        public void ReplaceQuestionWithNew(IQuestion oldQuestion, IQuestion newQuestion)
        {
            Guid oldQuestionId = oldQuestion.PublicKey;

            IComposite questionParent = this.GetParentOfQuestion(oldQuestionId);

            if (questionParent != null)
            {
                int indexOfQuestion = questionParent.Children.FindIndex(child => IsQuestionWithSpecifiedId(child, oldQuestionId));
                questionParent.Children[indexOfQuestion] = newQuestion;
            }
            else
            {
                logger.Warn(string.Format(
                    "Failed to replace question '{0}' with new because it's parent is not found.",
                    oldQuestionId));
            }
        }

        public void Remove(Guid itemKey, Guid? propagationKey, Guid? parentPublicKey, Guid? parentPropagationKey)
        {
            // we could delete group from the root of Questionnaire
            if (parentPublicKey == null || parentPublicKey == Guid.Empty || this.PublicKey == parentPublicKey)
            {
                this.Children.RemoveAll(i => i.PublicKey == itemKey);
            }
            else
            {
                IGroup parent = this.Find<IGroup>(g => g.PublicKey == parentPublicKey).FirstOrDefault();
                if (parent != null)
                {
                    parent.Children.RemoveAll(i => i.PublicKey == itemKey);
                }
            }
        }

        public void RemoveGroup(Guid groupId)
        {
            IComposite groupParent = this.GetParentOfGroup(groupId);

            if (groupParent != null)
            {
                var group = groupParent.Children.First(child => IsGroupWithSpecifiedId(child, groupId)) as IGroup;
                RemoveChildGroupBySpecifiedId(groupParent, groupId);
            }
            else
            {
                logger.Warn(string.Format("Failed to remove group '{0}' because it's parent is not found.", groupId));
            }
        }

        public void UpdateGroup(Guid groupId, string title, string description, string conditionExpression)
        {
            this.UpdateGroup(groupId, group =>
            {
                @group.ConditionExpression = conditionExpression;
                @group.Description = description;
                @group.Update(title);
            });
        }

        public void UpdateGroup(Guid groupId, Action<Group> update)
        {
            var group = this.Find<Group>(groupId);

            if (@group != null)
                update(@group);
        }

        public void RemoveQuestion(Guid questionId)
        {
            IComposite questionParent = this.GetParentOfQuestion(questionId);

            if (questionParent != null)
            {
                RemoveChildQuestionBySpecifiedId(questionParent, questionId);
            }
            else
            {
                logger.Warn(string.Format("Failed to remove question '{0}' because it's parent is not found.", questionId));
            }
        }

        private static void RemoveChildGroupBySpecifiedId(IComposite container, Guid groupId)
        {
            RemoveFirstChild(container, child => IsGroupWithSpecifiedId(child, groupId));
        }

        private static void RemoveChildQuestionBySpecifiedId(IComposite container, Guid questionId)
        {
            RemoveFirstChild(container, child => IsQuestionWithSpecifiedId(child, questionId));
        }

        private static void RemoveFirstChild(IComposite container, Predicate<IComposite> condition)
        {
            IComposite child = container.Children.Find(condition);

            if (child != null)
            {
                container.Children.Remove(child);
            }
        }

        private IComposite GetParentOfGroup(Guid groupId)
        {
            if (ContainsChildGroupWithSpecifiedId(this, groupId))
                return this;

            return this
                .Find<IGroup>(group => ContainsChildGroupWithSpecifiedId(group, groupId))
                .SingleOrDefault();
        }

        private IComposite GetParentOfItem(IComposite item)
        {
            if (ContainsChildItem(this, item))
                return this;

            return this
                .Find<IGroup>(group => ContainsChildItem(group, item))
                .SingleOrDefault();
        }

        public IGroup GetParentOfQuestion(Guid questionId)
        {
            return this
                .Find<IGroup>(group => ContainsChildQuestionWithSpecifiedId(group, questionId))
                .SingleOrDefault();
        }

        public IEnumerable<T> GetAllQuestions<T>(IGroup startGroup = null) where T : class, IComposite
        {
            var result = new List<T>();
            var groups = new Queue<IComposite>();
            groups.Enqueue(startGroup ?? this);

            while (groups.Count != 0)
            {
                IComposite queueItem = groups.Dequeue();
                var question = queueItem as T;
                if (question != null)
                {
                    result.Add(question);
                    continue;
                }

                foreach (IComposite child in queueItem.Children)
                {
                    groups.Enqueue(child);
                }
            }

            return result;
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

        private static bool ContainsChildGroupWithSpecifiedId(IComposite container, Guid groupId)
        {
            return container.Children.Any(child => IsGroupWithSpecifiedId(child, groupId));
        }

        private static bool ContainsChildQuestionWithSpecifiedId(IComposite container, Guid questionId)
        {
            return container.Children.Any(child => IsQuestionWithSpecifiedId(child, questionId));
        }

        private static bool ContainsChildItem(IComposite container, IComposite item)
        {
            return container.Children.Any(child => child == item);
        }

        private static bool IsGroupWithSpecifiedId(IComposite child, Guid groupId)
        {
            return child is IGroup && ((IGroup)child).PublicKey == groupId;
        }

        private static bool IsQuestionWithSpecifiedId(IComposite child, Guid questionId)
        {
            return child is IQuestion && ((IQuestion)child).PublicKey == questionId;
        }

        public void ConnectChildrenWithParent()
        {
            foreach (var item in this.Children)
            {
                item.SetParent(this);
                item.ConnectChildrenWithParent();
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

            sourceContainer.Children.Remove(item);

            if (targetIndex < 0)
            {
                //item.SetParent(targetContainer);
                targetContainer.Children.Insert(0, item);
            }
            else if (targetIndex >= targetContainer.Children.Count)
            {
                //item.SetParent(targetContainer);
                targetContainer.Children.Add(item);
            }
            else
            {
                targetContainer.Children.Insert(targetIndex, item);
            }
        }

        private IComposite GetItemOrLogWarning(Guid itemId)
        {
            var itemToMove = this.Find<IComposite>(item => item.PublicKey == itemId).FirstOrDefault();

            if (itemToMove == null)
            {
                logger.Warn(string.Format("Failed to locate item {0}.", itemId));
            }

            return itemToMove;
        }

        private IComposite GetParentOfItemOrLogWarning(IComposite item)
        {
            IComposite foundParent = this.GetParentOfItem(item);

            if (foundParent == null)
            {
                logger.Warn(string.Format("Failed to find parent of item {0}.", item.PublicKey));
            }

            return foundParent;
        }

        private IComposite GetGroupOrRootOrLogWarning(Guid? groupId)
        {
            if (groupId == null)
                return this;

            IComposite foundGroup = this.Find<IGroup>(group => group.PublicKey == groupId).FirstOrDefault();

            if (foundGroup == null)
            {
                logger.Warn(string.Format("Failed to find group {0}.", groupId.Value));
            }

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

            doc.Children = new List<IComposite>();
            foreach (var composite in this.Children)
            {
                doc.Children.Add(composite.Clone());
            }

            return doc;
        }

        public void UpdateRosterGroupsIfNeeded(List<Guid> triggeredGroupIds, Guid rosterSizeQuestionId)
        {
            if (triggeredGroupIds != null && triggeredGroupIds.Count > 0)
            {
                this.MarkGroupsAsRosterAndSetRosterSizeQuestion(triggeredGroupIds, rosterSizeQuestionId);
            }
        }

        public void MoveHeadQuestionPropertiesToRoster(Guid QuestionId, Guid? GroupPublicKey)
        {
            if (GroupPublicKey != null)
            {
                var foundGroup = this.Find<IGroup>(group => group.PublicKey == GroupPublicKey).FirstOrDefault() as Group;
                if (foundGroup == null)
                {
                    logger.Warn(string.Format("Failed to find group {0}.", GroupPublicKey)); 
                }

                foundGroup.RosterTitleQuestionId = QuestionId;

                if (foundGroup.RosterSizeQuestionId != null)
                {
                    var scopeGroups = this.Find<IGroup>(group => group.RosterSizeQuestionId == foundGroup.RosterSizeQuestionId);
                    foreach (var scopeGroup in scopeGroups)
                    {
                        var @group = scopeGroup as Group;
                        if (@group != null)
                            @group.RosterTitleQuestionId = QuestionId;
                    }
                }
            }
        }


        public void RemoveHeadPropertiesFromRosters(Guid QuestionId)
        {
            var scopeGroups = this.Find<IGroup>(group => group.RosterTitleQuestionId == QuestionId);  
          
            foreach (var scopeGroup in scopeGroups)
                    {
                        var @group = scopeGroup as Group;
                        if (@group != null)
                            @group.RosterTitleQuestionId = null;
                    }
        }


        public void MarkGroupsAsRosterAndSetRosterSizeQuestion(List<Guid> triggeredGroupIds, Guid rosterSizeQuestionId)
        {
            foreach (var triggeredGroupId in triggeredGroupIds)
            {
                var triggeredGroup = this.Find<IGroup>(group => group.PublicKey == triggeredGroupId).FirstOrDefault() as Group;

                if (triggeredGroup == null)
                {
                    logger.Warn(string.Format("Failed to find group [{0}]. This groups was used in triggers in [{1}]s question", triggeredGroupId, rosterSizeQuestionId));
                    continue;
                }

                triggeredGroup.IsRoster = true;
                triggeredGroup.RosterSizeQuestionId = rosterSizeQuestionId;
            }
        }
    }
}