using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Main.Core.Entities.Composite;
using WB.Core.SharedKernels.SurveySolutions.Documents;

namespace Main.Core.Entities.SubEntities
{
    [DebuggerDisplay("Group {PublicKey}")]
    public class Group : IGroup
    {
        public Group()
        {
            this.PublicKey = Guid.NewGuid();
            this.Children = new List<IComposite>();
            this.ConditionExpression = string.Empty;
            this.Description = string.Empty;
            this.Enabled = true;
            this.FixedRosterTitles = new FixedRosterTitle[0];
        }

        public Group(string text)
            : this()
        {
            this.Title = text;
        }

        public List<IComposite> Children { get; set; }

        public string ConditionExpression { get; set; }

        public bool Enabled { get; set; }

        public string Description { get; set; }

        public string VariableName { get; set; }

        public bool IsRoster { get; set; }

        public Guid? RosterSizeQuestionId { get; set; }

        public RosterSizeSourceType RosterSizeSource { get; set; }

        [Obsolete]
        public string[] RosterFixedTitles 
        {
            set
            {
                if (value != null && value.Any())
                {
                    FixedRosterTitles = value.Select((t, i) => new FixedRosterTitle(i, t)).ToArray();
                }
                else
                {
                    FixedRosterTitles = new FixedRosterTitle[0];
                }
            } 
        }

        public FixedRosterTitle[] FixedRosterTitles { get; set; }

        public Guid? RosterTitleQuestionId { get; set; }

        private IComposite parent;

        public Propagate Propagated { get; set; }

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

        public void Insert(IComposite c, Guid? afterItem)
        {
            try
            {
                int index = this.Children.FindIndex(0, this.Children.Count, x => x.PublicKey == afterItem);
                if (index != -1)
                {
                    this.Children.Insert(index + 1, c);
                    return;
                }
                
                this.Children.Insert(0, c);
                return;
            }
            catch (CompositeException)
            {
            }

            throw new CompositeException();
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
                Propagated = this.Propagated,
                PublicKey = this.PublicKey,
                Title = this.Title,
                VariableName = this.VariableName,
                IsRoster = this.IsRoster,
                RosterSizeQuestionId = this.RosterSizeQuestionId,
                RosterSizeSource = this.RosterSizeSource,
                RosterTitleQuestionId = this.RosterTitleQuestionId,
                FixedRosterTitles = this.FixedRosterTitles
            };

            foreach (var composite in this.Children)
            {
                newGroup.Children.Add(composite.Clone());
            }

            return newGroup;
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