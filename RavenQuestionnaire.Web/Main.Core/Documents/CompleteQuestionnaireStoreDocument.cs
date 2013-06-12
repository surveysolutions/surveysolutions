using WB.Core.Infrastructure;

namespace Main.Core.Documents
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using Main.Core.AbstractFactories;
    using Main.Core.Entities.Composite;
    using Main.Core.Entities.Extensions;
    using Main.Core.Entities.SubEntities;
    using Main.Core.Entities.SubEntities.Complete;
    using Main.DenormalizerStorage;

    [SmartDenormalizer]
    public class CompleteQuestionnaireStoreDocument : ICompleteQuestionnaireDocument, IView
    {
        private readonly List<Guid> triggers = new List<Guid>();
        private IComposite parent;
        private GroupHash questionHash;

        public CompleteQuestionnaireStoreDocument()
        {
            this.CreationDate = DateTime.Now;
            this.LastEntryDate = DateTime.Now;

            //// this.PublicKey = Guid.NewGuid();
            this.Children = new List<IComposite>();
            this.StatusChangeComments = new List<ChangeStatusDocument>();
        }

        #region Public Properties

        public List<IComposite> Children { get; set; }

        public DateTime? CloseDate { get; set; }

        public string ConditionExpression { get; set; }

        public DateTime CreationDate { get; set; }

        public UserLight Creator { get; set; }

        public string Description { get; set; }

        public DateTime EnableStateCalculated { get; set; }

        public bool Enabled { get; set; }

        public Guid? ForcingPropagationPublicKey
        {
            get
            {
                return null;
            }
        }

        public DateTime LastEntryDate { get; set; }

        public VisitedGroup LastVisitedGroup { get; set; }

        public DateTime? OpenDate { get; set; }

        public Guid? CreatedBy { get; set; }

        public bool IsPublic { get; set; }

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

        public Guid? PropagationPublicKey
        {
            get
            {
                return null;
            }

            set
            {
            }
        }

        public Guid PublicKey { get; set; }

        public UserLight Responsible { get; set; }

        public SurveyStatus Status { get; set; }

        public string StatusChangeComment { get; set; }

        public List<ChangeStatusDocument> StatusChangeComments { get; set; }

        public Guid TemplateId { get; set; }

        public string Title { get; set; }

        public List<Guid> Triggers
        {
            get
            {
                return this.triggers;
            }

            set
            {
            }
        }

        #endregion

        #region Properties

        #warning ReadLayer: this is now serialized to DB. but should not
        private GroupHash QuestionHash
        {
            get
            {
                return this.questionHash ?? (this.questionHash = new GroupHash(this));
            }
        }

        #endregion

        #region Public Methods and Operators

        public static explicit operator CompleteQuestionnaireStoreDocument(QuestionnaireDocument doc)
        {
            var result = new CompleteQuestionnaireStoreDocument
                {
                    TemplateId = doc.PublicKey, 
                    Title = doc.Title, 
                    Triggers = doc.Triggers, 
                    ConditionExpression = doc.ConditionExpression, 
                    Description = doc.Description
                };

            foreach (IComposite child in doc.Children)
            {
                var question = child as IQuestion;
                if (question != null)
                {
                    result.Children.Add(new CompleteQuestionFactory().ConvertToCompleteQuestion(question));
                    continue;
                }

                var group = child as IGroup;
                if (group != null)
                {
                    result.Children.Add(new CompleteGroupFactory().ConvertToCompleteGroup(group));
                    continue;
                }

                throw new InvalidOperationException("unknown children type");
            }

            /*   foreach (IQuestion question in doc.Questions)
               {
                   result.Questions.Add(new CompleteQuestionFactory().ConvertToCompleteQuestion(question));
               }
               foreach (IGroup group in doc.Groups)
               {
                   result.Groups.Add(new CompleteGroupFactory().ConvertToCompleteGroup(group));
               }**/
            return result;
        }

        public static explicit operator CompleteQuestionnaireStoreDocument(CompleteQuestionnaireDocument doc)
        {
            var result = new CompleteQuestionnaireStoreDocument
                {
                    PublicKey = doc.PublicKey, 
                    TemplateId = doc.TemplateId, 
                    Title = doc.Title, 
                    Triggers = doc.Triggers, 
                    ConditionExpression = doc.ConditionExpression, 
                    CreationDate = doc.CreationDate, 
                    LastEntryDate = doc.LastEntryDate, 
                    Status = doc.Status, 
                    Creator = doc.Creator, 
                    Responsible = doc.Responsible, 
                    StatusChangeComment = doc.StatusChangeComment, 
                    PropagationPublicKey = doc.PropagationPublicKey, 
                    Description = doc.Description
                };

            result.StatusChangeComments.Add(
                new ChangeStatusDocument
                    {
                        Status = doc.Status, 
                        Responsible = doc.Creator, 
                        ChangeDate = doc.CreationDate // not sure it's correct
                    });

            result.Children.AddRange(doc.Children.ToList());
            return result;
        }

        ////  public List<IObserver<CompositeInfo>> Observers { get; set; }

        public virtual void Add(IComposite c, Guid? parentKey, Guid? parentPropagationKey)
        {
            var group = c as ICompleteGroup;

            if (group == null || !group.PropagationPublicKey.HasValue)
            {
                throw new ArgumentException("Only propagated group can be added.");
            }

            IComposite itemToadd = null;

            if (parentKey == null || this.PublicKey == parentKey)
            {
                itemToadd = this;
            }
            else
            {
                itemToadd =
                    this.Find<ICompleteGroup>(
                        g => g.PublicKey == parentKey && g.PropagationPublicKey == parentPropagationKey).FirstOrDefault(
                            );
            }

            if (itemToadd != null)
            {
                itemToadd.Children.Add(c);
                this.QuestionHash.AddGroup(group);
                return;
            }

            /*if (c is ICompleteGroup && ((ICompleteGroup)c).PropagationPublicKey.HasValue && !parent.HasValue)
            {
                if (this.Children.Count(g => g.PublicKey.Equals(c.PublicKey)) > 0)
                {
                    c.Parent = this;
                    this.Children.Add(c);
                    return;
                }
            }

            foreach (IComposite completeGroup in this.Children)
            {
                try
                {
                    //// c.Parent = completeGroup;
                    completeGroup.Add(c, parent, null);
                    this.QuestionHash.AddGroup(c as ICompleteGroup);
                    return;
                }
                catch (CompositeException)
                {
                }
            }*/
            throw new CompositeException();
        }

        public IComposite Clone()
        {
            throw new NotImplementedException();
        }

        public void ConnectChildsWithParent()
        {
            foreach (IComposite item in this.Children)
            {
                item.SetParent(this);
                item.ConnectChildsWithParent();
            }
        }

        public T Find<T>(Guid publicKey) where T : class, IComposite
        {
            T resultInsideGroups =
                this.Children.Select(answer => answer.Find<T>(publicKey)).FirstOrDefault(result => result != null);
            if (resultInsideGroups != null)
            {
                return resultInsideGroups;
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

        public IEnumerable<ICompleteQuestion> GetFeaturedQuestions()
        {
            return this.QuestionHash.GetFeaturedQuestions();
        }

        public IComposite GetParent()
        {
            return this.parent;
        }

        public ICompleteQuestion GetQuestion(Guid publicKey, Guid? propagationKey)
        {
            return this.QuestionHash.GetQuestion(publicKey, propagationKey);
        }

        public CompleteQuestionWrapper GetQuestionByKey(string key)
        {
            return this.QuestionHash.GetQuestionByKey(key);
        }

        public CompleteQuestionWrapper GetQuestionWrapper(Guid publicKey, Guid? propagationKey)
        {
            return this.QuestionHash.GetQuestionWrapper(publicKey, propagationKey);
        }

        public IEnumerable<ICompleteQuestion> GetQuestions()
        {
            return this.QuestionHash.Questions;
        }

        public bool HasVisibleItemsForScope(QuestionScope questionScope)
        {
            return true;
        }

        /*public void Remove(IComposite c)
        {
            this.RemoveInt(c);
            this.QuestionHash.RemoveGroup(c as ICompleteGroup);
        }*/

        /*private void RemoveInt(IComposite c)
        {
            var propogate = c as ICompleteGroup;
            if (propogate != null && propogate.PropagationPublicKey.HasValue)
            {
                bool isremoved = false;
                List<IComposite> propagatedGroups =
                    this.Children.Where(
                        g =>
                        g.PublicKey.Equals(propogate.PublicKey)
                        && ((ICompleteGroup)g).PropagationPublicKey == propogate.PropagationPublicKey).ToList();
                foreach (ICompleteGroup propagatableCompleteGroup in propagatedGroups)
                {
                    this.Children.Remove(propagatableCompleteGroup);
                    isremoved = true;
                }

                if (isremoved)
                {
                    return;
                }
            }

            foreach (IComposite completeGroup in this.Children)
            {
                try
                {
                    completeGroup.Remove(c);
                    return;
                }
                catch (CompositeException)
                {
                }
            }

            throw new CompositeException();
        }*/

        /*public void Remove(Guid publicKey, Guid? propagationKey)
        {
            IComposite forRemove = this.Children.FirstOrDefault(g => g.PublicKey.Equals(publicKey));
            if (forRemove != null && forRemove is ICompleteGroup && ((ICompleteGroup)forRemove).PropagationPublicKey.HasValue)
            {
                this.Children.Remove(forRemove);
                return;
            }

            foreach (IComposite completeGroup in this.Children)
            {
                try
                {
                    completeGroup.Remove(publicKey, null);
                    return;
                }
                catch (CompositeException)
                {
                }
            }

            throw new CompositeException();
        }*/

        public void Remove(Guid itemKey, Guid? propagationKey, Guid? parentPublicKey, Guid? parentPropagationKey)
        {
            // only propagate group is allowed to be remove
            if (!parentPublicKey.HasValue)
            {
                throw new ArgumentException("Parent was not set.");
            }

            ICompleteGroup parent =
                this.Find<ICompleteGroup>(
                    g => g.PublicKey == parentPublicKey && g.PropagationPublicKey == parentPropagationKey).
                    FirstOrDefault();

            ICompleteGroup itemToDelete =
                parent.Children.Where(i => i.PublicKey == itemKey).Select(a => a as ICompleteGroup).FirstOrDefault(
                    b => b.PropagationPublicKey == propagationKey);

            parent.Children.Remove(itemToDelete);
            this.QuestionHash.RemoveGroup(itemToDelete);
        }

        public void SetParent(IComposite parent)
        {
            this.parent = parent;
        }

        public IEnumerable<CompleteQuestionWrapper> WrappedQuestions()
        {
            return this.QuestionHash.WrapedQuestions;
        }

        #endregion
    }
}