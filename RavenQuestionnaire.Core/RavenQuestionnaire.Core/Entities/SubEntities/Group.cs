using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using RavenQuestionnaire.Core.Entities.Composite;
using RavenQuestionnaire.Core.Entities.Observers;

namespace RavenQuestionnaire.Core.Entities.SubEntities
{
    public enum Propagate
    {
        None,
        Propagated,
        AutoPropagated
    }

    public interface IGroup : IComposite, ITriggerable
    {
        string Title { get; set; }
        Propagate Propagated { get; set; }
        bool IsValid { get; set; }
        string ConditionExpression { get; set; }
        //bool Enabled { get; set; }
    }
    public class Group : IGroup
    {
        public Group()
        {
            this.PublicKey = Guid.NewGuid();
            this.Children = new List<IComposite>();
            this.Triggers = new List<Guid>();
            this.ConditionExpression = string.Empty;
        }
        
        public Group(string text)
            : this()
        {
            this.Title = text;
        }

        public Guid PublicKey { get; set; }
        public string Title { get; set; }
        public bool IsValid { get; set; }
        public string ConditionExpression { get; set; }
        public bool Enabled { get; set; }

        public Propagate Propagated { get; set; }

        public List<Guid> Triggers { get; set; }

        public void Update(string groupText)
        {
            this.Title = groupText;
        }
        public void Add(IComposite c, Guid? parent)
        {
            if ((parent.HasValue && parent.Value == PublicKey) || !parent.HasValue)
            {

                Children.Add(c);
                return;

            }
            foreach (IComposite child in Children)
            {
                try
                {
                    child.Add(c, parent);
                    return;
                }
                catch (CompositeException)
                {
                }
            }
            throw new CompositeException();
        }

        public void Insert(IComposite c, Guid? afterItem)
        {
            try
            {
                int index = this.Children.FindIndex(0, this.Children.Count, x => x.PublicKey == afterItem);
                if (index != -1)
                {
                    this.Children.Insert(index+1, c);
                    return;
                }
                else
                {
                    this.Children.Add(c);
                    return;
                }
            }
            catch (CompositeException)
            {
            }
            throw  new CompositeException();

        }

        public void Remove(IComposite c)
        {
            this.Remove(c.PublicKey);
        }

        public void Remove(Guid publicKey)
        {
            var group = this.Children.FirstOrDefault(g =>  g.PublicKey.Equals(publicKey));
            if (group != null)
            {
                this.Children.Remove(group);
                return;
            }
            foreach (IComposite child in this.Children)
            {
                try
                {
                    child.Remove(publicKey);
                    return;
                }
                catch (CompositeException)
                {

                }
            }
            throw new CompositeException();
        }

        public T Find<T>(Guid publicKey) where T : class, IComposite
        {
            foreach (IComposite child in Children)
            {
                if (child is T && child.PublicKey == publicKey)
                    return child as T;
                T subNodes = child.Find<T>(publicKey);
                if (subNodes != null)
                    return subNodes;
            }
            return null;
        }

        public IEnumerable<T> Find<T>(Func<T, bool> condition) where T : class
        {
            return
                Children.Where(a => a is T && condition(a as T)).Select(a => a as T).Union(
                    Children.SelectMany(q => q.Find<T>(condition)));

        }

        public T FirstOrDefault<T>(Func<T, bool> condition) where T : class
        {
            return Children.Where(a => a is T && condition(a as T)).Select(a => a as T).FirstOrDefault() ??
                   Children.SelectMany(q => q.Find<T>(condition)).FirstOrDefault();
        }

        public List<IComposite> Children { get; set; }
        [JsonIgnore]
        public IComposite Parent
        {
            get {throw new NotImplementedException(); }
        }

        [JsonIgnore]
        public IComposite ParentGroup { get; set; }

    }
}
