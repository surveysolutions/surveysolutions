using System;
using System.Linq;
using Newtonsoft.Json;
using System.Collections.Generic;
using RavenQuestionnaire.Core.Entities.Composite;
using RavenQuestionnaire.Core.Entities.SubEntities;

namespace RavenQuestionnaire.Core.Documents
{
    public interface IQuestionnaireDocument : IGroup
    {
        string Id { get; set; }
        DateTime CreationDate { get; set; }
        DateTime LastEntryDate { get; set; }
        DateTime? OpenDate { get; set; }
        DateTime? CloseDate { get; set; }
    }


    public class QuestionnaireDocument : IQuestionnaireDocument
    {
        public QuestionnaireDocument()
        {
            CreationDate = DateTime.Now;
            LastEntryDate = DateTime.Now;
            PublicKey = Guid.NewGuid();
            Children = new List<IComposite>();
        }

        public string Id { get; set; }
       
        public string Title { get; set; }
        public Guid PublicKey
        {
            get { return publicKey; }
            set
            {
                publicKey = value;
                this.Id = value.ToString();
            }
        }

        private Guid publicKey;
        [JsonIgnore]
        public Propagate Propagated
        {
            get { return Propagate.None; }
            set {  }
        }
        [JsonIgnore]
        public bool IsValid { get; set; }

        public List<Guid> Triggers
        {
            get { return triggers; }
            set { }
        }

        private List<Guid> triggers = new List<Guid>();


        public DateTime CreationDate
        { get; set; }
        public DateTime LastEntryDate
        { get; set; }

        public DateTime? OpenDate
        { get; set; }

        public DateTime? CloseDate { get; set; }


        #region Implementation of IComposite
        public void Add(IComposite c, Guid? parent)
        {
            if (!parent.HasValue)
            {

                this.Children.Add(c);
                return;

            }
            foreach (IComposite child in this.Children)
            {
                try
                {
                    child.Add(c, parent);
                    return;
                }
                catch (CompositeException)
                {
                }
                /* if (child.Add(c, parent))
                     return true;*/
            }
            
            throw new CompositeException();
        }

        public void Remove(IComposite c)
        {
           this.Remove(c.PublicKey);
        }
        public void Remove(Guid publicKey)
        {
            var group = this.Children.FirstOrDefault(g => g.PublicKey.Equals(publicKey));
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

            /*   foreach (Group child in innerDocument.Groups)
               {
                   if (child is T && condition(child))
                       return child as T;
                   T subNodes = child.Find<T>(condition);
                   if (subNodes != null)
                       return subNodes;
               }
               foreach (Question child in innerDocument.Questions)
               {
                   if (child is T && condition(child))
                       return child as T;
                   T subNodes = child.Find<T>(condition);
                   if (subNodes != null)
                       return subNodes;
               }
               return null;*/
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
            get { throw new NotImplementedException(); }
        }
        #endregion
    }
}
