using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using RavenQuestionnaire.Core.Entities.Composite;

namespace RavenQuestionnaire.Core.Entities.SubEntities.Complete
{
    public class BindedCompleteQuestion : ICompleteQuestion, IBinded
    {
        public BindedCompleteQuestion()
        {
            PublicKey = Guid.NewGuid();
            Children=new List<IComposite>();
        }
/*
        public BindedCompleteQuestion(ICompleteQuestion template)
        {
            this.ParentPublicKey = template.PublicKey;
        }*/
        public static explicit operator BindedCompleteQuestion(BindedQuestion doc)
        {
            BindedCompleteQuestion result = new BindedCompleteQuestion
            {
                PublicKey = doc.PublicKey,
                ParentPublicKey = doc.ParentPublicKey
            };
            return result;
        }
        public void Copy(ICompleteQuestion template)
        {
            var witAnswers = template as ICompleteQuestion;
            if (witAnswers != null)
                this.Children = witAnswers.Children;
            this.QuestionText = template.QuestionText;
            this.QuestionType = template.QuestionType;

        }

        #region Implementation of IComposite

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

        public List<IComposite> Children { get; set; }
        [JsonIgnore]
        public IComposite Parent
        {
            get { throw new NotImplementedException(); }
        }

        #endregion

        #region Implementation of IQuestion

        public object Answer { get; set; }
        public void SetAnswer(object answer)
        {
            
        }

        public Guid PublicKey { get; set; }
        public string QuestionText { get; set; }

        public QuestionType QuestionType { get; set; }

        public string ConditionExpression { get; set; }

        public string ValidationExpression
        { get; set; }

        public string StataExportCaption { get; set; }

        public string Instructions { get; set; }

        public List<Image> Cards { get; set; }

        public Order AnswerOrder { get; set; }

        public bool Featured
        {
            get { return false; }
            set { }
        }

        public Dictionary<string, object> Attributes
        {
            get { return new Dictionary<string, object>(0); }
            set { }
        }

        #endregion

        #region Implementation of ICompleteQuestion

        public Guid? PropogationPublicKey
        {
            get { throw new NotImplementedException(); }
            set { throw new NotImplementedException(); }
        }

        public bool Enabled
        {
            get { return false; }
            set { }
        }

        public bool Valid
        {
            get { return true; }
            set { }
        }

        public DateTime? AnswerDate { get; set; }

        #endregion

        #region Implementation of IBinded

        public Guid ParentPublicKey { get; set; }

        #endregion

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
