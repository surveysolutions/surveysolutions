using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using RavenQuestionnaire.Core.Entities.Composite;

namespace RavenQuestionnaire.Core.Entities.SubEntities.Complete
{
    public class BindedCompleteQuestion : ICompleteQuestion<ICompleteAnswer>, IBinded
    {
        public BindedCompleteQuestion()
        {
            this.template = new CompleteQuestion();
            PublicKey = Guid.NewGuid();
        }

        public BindedCompleteQuestion(ICompleteQuestion<ICompleteAnswer> template)
        {
            this.template=template;
        }
        private readonly ICompleteQuestion<ICompleteAnswer> template;
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

        #endregion

        #region Implementation of IQuestion

        public Guid PublicKey { get; set; }
        [XmlIgnore]
        public string QuestionText
        {
            get { return template.QuestionText; }
            set { throw new InvalidOperationException("question text can't be changed at binded question"); }
        }
        [XmlIgnore]
        public QuestionType QuestionType
        {
            get { return template.QuestionType; }
            set { throw new InvalidOperationException("QuestionType can't be changed at binded question"); }
        }
        [XmlIgnore]
        public string ConditionExpression
        {
            get { return template.QuestionText; }
            set { throw new InvalidOperationException("ConditionExpression can't be changed at binded question"); }
        }
        [XmlIgnore]
        public string StataExportCaption
        {
            get { return template.QuestionText; }
            set { throw new InvalidOperationException("StataExportCaption can't be changed at binded question"); }
        }

        #endregion

        #region Implementation of ICompleteQuestion
        [XmlIgnore]
        public bool Enabled
        {
            get { return false; }
            set { throw new InvalidOperationException("StataExportCaption can't be changed at binded question"); }
        }

        #endregion

        #region Implementation of IQuestion<CompleteAnswer>

        [XmlIgnore]
        public List<ICompleteAnswer> Answers
        {
            get { return template.Answers; }
            set { throw new InvalidOperationException("Answers can't be changed at binded question"); }
        }

        #endregion

        #region Implementation of IBinded

        public Guid ParentPublicKey
        {
            get { return template.PublicKey; }
            set { template.PublicKey = value; }
        }

        #endregion
    }
}
