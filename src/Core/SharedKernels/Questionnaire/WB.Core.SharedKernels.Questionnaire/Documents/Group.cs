using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using Main.Core.Entities.Composite;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.SharedKernels.SurveySolutions.Documents;

namespace Main.Core.Entities.SubEntities
{
    [DebuggerDisplay("Group {PublicKey}")]
    public class Group : IGroup
    {
        public Group(string title = null, List<IComposite> children = null)
        {
            this.Title = title;

            this.PublicKey = Guid.NewGuid();
            this.children = new List<IComposite>();
            this.ConditionExpression = string.Empty;
            this.Description = string.Empty;
            this.Enabled = true;
            this.FixedRosterTitles = new FixedRosterTitle[0];

            if (children == null)
                this.children = new List<IComposite>();
            else
            {
                this.children = children;
                this.ConnectChildrenWithParent();
            }
        }
        
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
            }
        }

        public string ConditionExpression { get; set; }

        public bool HideIfDisabled { get; set; }

        public bool Enabled { get; set; }

        public string Description { get; set; }

        public string VariableName { get; set; }

        public bool IsRoster { get; set; }

        public Guid? RosterSizeQuestionId { get; set; }

        public RosterSizeSourceType RosterSizeSource { get; set; }

        public FixedRosterTitle[] FixedRosterTitles { get; set; }

        public Guid? RosterTitleQuestionId { get; set; }

        private IComposite parent;

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

        public T Find<T>(Guid publicKey) where T : class, IComposite
        {
            foreach (IComposite child in this.children)
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

        public void Insert(int index, IComposite itemToInsert, Guid? parent)
        {
            if (index < 0)
            {
                this.children.Insert(0, itemToInsert);
            }
            else if (index >= this.children.Count)
            {
                this.children.Add(itemToInsert);
            }
            else
                this.children.Insert(index, itemToInsert);

            itemToInsert.SetParent(this);
            itemToInsert.ConnectChildrenWithParent();
        }

        public void RemoveChild(Guid childId)
        {
            IComposite child = this.children.Find(c => c.PublicKey == childId);
            this.children.Remove(child);
        }

        public void ConnectChildrenWithParent()
        {
            foreach (var item in this.Children)
            {
                item.SetParent(this);
                item.ConnectChildrenWithParent();
            }
        }

        public IComposite Clone()
        {
            var newGroup = new Group
            {
                ConditionExpression = this.ConditionExpression,
                Description = this.Description,
                Enabled = this.Enabled,
                PublicKey = this.PublicKey,
                Title = this.Title,
                VariableName = this.VariableName,
                IsRoster = this.IsRoster,
                HideIfDisabled = this.HideIfDisabled,
                RosterSizeQuestionId = this.RosterSizeQuestionId,
                RosterSizeSource = this.RosterSizeSource,
                RosterTitleQuestionId = this.RosterTitleQuestionId,
                FixedRosterTitles = this.FixedRosterTitles.Select(x => new FixedRosterTitle(x.Value, x.Title)).ToArray()
            };

            var clonnedChildren = new List<IComposite>();

            foreach (var composite in this.children)
            {
                clonnedChildren.Add(composite.Clone());
            }
            newGroup.Children = clonnedChildren.ToReadOnlyCollection();

            return newGroup;
        }

        public void ReplaceChildEntityById(Guid id, IComposite newEntity)
        {
            int indexOfEntity = this.children.FindIndex(child => child.PublicKey == id);
            this.children[indexOfEntity] = newEntity;
            newEntity.SetParent(this);
        }

        public void Update(string groupText)
        {
            this.Title = groupText;
        }

        public override string ToString()
        {
            return string.Format("Group {{{0}}} '{1}'", this.PublicKey, this.Title ?? "<untitled>");
        }
    }
}
