using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using Newtonsoft.Json;
using RavenQuestionnaire.Core.Entities.Composite;
using RavenQuestionnaire.Core.Entities.Observers;
using RavenQuestionnaire.Core.Entities.SubEntities.Complete;

namespace RavenQuestionnaire.Core.Entities.SubEntities
{
    public class BindedQuestion : IQuestion<IAnswer>, IBinded
    {
        public BindedQuestion()
        {
            PublicKey = Guid.NewGuid();
            Answers=new List<IAnswer>();
        }
        public BindedQuestion(IQuestion<IAnswer> template)
        {
            this.ParentPublicKey=template.PublicKey;
        }


        public Guid PublicKey { get; set; }

        public Guid ParentPublicKey { get; set; }

        [JsonIgnore]
        public string QuestionText { get; set; }

        [JsonIgnore]
        public QuestionType QuestionType { get; set; }

        [JsonIgnore]
        public List<IAnswer> Answers { get; set; }

        [JsonIgnore]
        public string ConditionExpression { get; set; }

        [JsonIgnore]
        public string StataExportCaption { get; set; }

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

        public T FirstOrDefault<T>(Func<T, bool> condition) where T : class
        {
            return null;
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
