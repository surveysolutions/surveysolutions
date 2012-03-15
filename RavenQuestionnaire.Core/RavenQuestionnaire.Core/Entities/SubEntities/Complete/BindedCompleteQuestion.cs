using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using Newtonsoft.Json;
using RavenQuestionnaire.Core.Entities.Composite;
using RavenQuestionnaire.Core.Entities.Observers;

namespace RavenQuestionnaire.Core.Entities.SubEntities.Complete
{
    public class BindedCompleteQuestion : ICompleteQuestion<ICompleteAnswer>, IBinded
    {
        public BindedCompleteQuestion()
        {
            PublicKey = Guid.NewGuid();
            Answers = new List<ICompleteAnswer>();
        }

        public BindedCompleteQuestion(ICompleteQuestion<ICompleteAnswer> template)
        {
            this.ParentPublicKey=template.PublicKey;
        }
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
            var witAnswers = template as ICompleteQuestion<ICompleteAnswer>;
            if (witAnswers != null)
                this.Answers = witAnswers.Answers;
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

        public void Remove<T>(Guid publicKey) where T : class, IComposite
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


        #endregion
        
        #region Implementation of IQuestion

        public Guid PublicKey { get; set; }
        public string QuestionText { get; set; }

        public QuestionType QuestionType { get; set; }

        public string ConditionExpression { get; set; }

        public string StataExportCaption { get; set; }

        #endregion

        #region Implementation of ICompleteQuestion
        
        public bool Enabled
        {
            get { return false; }
            set { }
        }

        #endregion

        #region Implementation of IQuestion<CompleteAnswer>

        public List<ICompleteAnswer> Answers { get; set; }

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

        public List<Guid> Triggers
        {
            get { return new List<Guid>(0); }
            set { }
        }
        #endregion
    }
}
