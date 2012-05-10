using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using RavenQuestionnaire.Core.Entities.Composite;
using RavenQuestionnaire.Core.Entities.SubEntities.Complete;

namespace RavenQuestionnaire.Core.Entities.SubEntities
{
    public class BindedQuestion : IQuestion, IBinded
    {
        public BindedQuestion()
        {
            PublicKey = Guid.NewGuid();
            this.Children=new List<IComposite>();
        }
        //public BindedQuestion(IQuestion template)
        //{
        //    this.ParentPublicKey=template.PublicKey;
        //}


        public Guid PublicKey { get; set; }

        public Guid ParentPublicKey { get; set; }

        [JsonIgnore]
        public string QuestionText { get; set; }

        [JsonIgnore]
        public QuestionType QuestionType { get; set; }


        [JsonIgnore]
        public List<Image> Cards { get; set; }

        [JsonIgnore]
        public Order AnswerOrder { get; set; }

        public bool Featured
        {
            get { return false; }
            set { }
        }

        public Dictionary<string, object> Attributes
        {
            get { return new Dictionary<string, object>(0);}
            set {  }
        }

        [JsonIgnore]
        public string ConditionExpression { get; set; }
        [JsonIgnore]
        public string ValidationExpression
        { get; set; }

        [JsonIgnore]
        public string StataExportCaption { get; set; }

        [JsonIgnore]
        public string Instructions
        {
            get; set; }

        public void Add(IComposite c, Guid? parent)
        {
            throw new CompositeException();
        }

        public void Remove(IComposite c)
        {
            throw new CompositeException();
        }
        public void Remove(Guid publicKey)
        {
            throw new CompositeException();
        }

        public T Find<T>(Guid publicKey) where T : class, IComposite
        {
            return null;
        }

        public IEnumerable<T> Find<T>(Func<T, bool> condition) where T : class
        {
            return new T[0];
        }

        public T FirstOrDefault<T>(Func<T, bool> condition) where T : class
        {
            return null;
        }

        [JsonIgnore]
        public List<IComposite> Children { get; set; }
        [JsonIgnore]
        public IComposite Parent
        {
            get { throw new NotImplementedException(); }
        }

        #region Implementation of IObservable<out CompositeEventArgs>

        public IDisposable Subscribe(IObserver<CompositeEventArgs> observer)
        {
            return null;
        }

        #endregion

        #region Implementation of ITriggerable
        [JsonIgnore]
        public List<Guid> Triggers
        {
            get { return new List<Guid>(0); }
            set { throw new InvalidOperationException(); }
        }

        #endregion
    }
}
