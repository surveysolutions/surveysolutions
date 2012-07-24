using System;
using Newtonsoft.Json;
using System.Collections.Generic;
using RavenQuestionnaire.Core.Entities.Composite;

namespace RavenQuestionnaire.Core.Entities.SubEntities.Complete
{
    public abstract class AbstractCompleteQuestion:ICompleteQuestion
    {
        #region Properties

        public Guid PublicKey { get; set; }

        public string QuestionText { get; set; }

        public QuestionType QuestionType { get; set; }

        public string ConditionExpression { get; set; }

        public string ValidationExpression { get; set; }

        public string StataExportCaption { get; set; }

        public string Instructions { get; set; }

        public List<Image> Cards { get; set; }

        public Order AnswerOrder { get; set; }

        public bool Featured { get; set; }

        public bool Mandatory { get; set; }

        public Guid? PropogationPublicKey { get; set; }

        public bool Enabled { get; set; }

        public bool Valid { get; set; }

        public DateTime? AnswerDate { get; set; }

        public abstract object Answer { get; set; }
        public void SetAnswer(object answer)
        {
            this.Answer = answer;
            this.AnswerDate = DateTime.Now;
        }

        public abstract string GetAnswerString();
        public abstract object GetAnswerObject();

        public string Comments { get; set; }
        public void SetComments(string comments)
        {
            this.Comments = comments;
        }
        [JsonIgnore]
        public IComposite Parent
        {
            get { throw new NotImplementedException(); }
        }

        public abstract List<IComposite> Children { get; set; }

        #endregion

        #region Constructor

        protected AbstractCompleteQuestion()
        {
            this.PublicKey = Guid.NewGuid();
            this.Enabled = true;
            this.Valid = true;
            this.Cards = new List<Image>();
            this.Triggers = new List<Guid>();
            //this.Answers.GetObservablePropertyChanges().Subscribe(e=>e.EventArgs)
        }

        protected AbstractCompleteQuestion(string text)
            : this()
        {

            this.QuestionText = text;
        }

        #endregion
        
        #region Implementation of ITriggerable

        public List<Guid> Triggers { get; set; }

        #endregion
        
        #region Method
        
        public abstract void Add(IComposite c, Guid? parent);

        public abstract void Remove(IComposite c);

        public abstract void Remove(Guid publicKey);

        public abstract T Find<T>(Guid publicKey) where T : class, IComposite;

        public abstract IEnumerable<T> Find<T>(Func<T, bool> condition) where T : class;

        public abstract T FirstOrDefault<T>(Func<T, bool> condition) where T : class;

        #endregion
    }
}
