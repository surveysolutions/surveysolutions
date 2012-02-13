using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using Newtonsoft.Json;
using RavenQuestionnaire.Core.Entities.Composite;
using RavenQuestionnaire.Core.Entities.SubEntities.Complete;

namespace RavenQuestionnaire.Core.Entities.SubEntities
{
    public class BindedQuestion : IQuestion<IAnswer>, IBinded
    {
        public BindedQuestion()
        {
            this.template=new Question();
            PublicKey = Guid.NewGuid();
        }
        public BindedQuestion(IQuestion<IAnswer> template)
        {
            this.template=template;
        }

        private readonly IQuestion<IAnswer> template;

        public Guid PublicKey { get; set; }

        public Guid ParentPublicKey
        {
            get { return template.PublicKey; }
            set { template.PublicKey = value; }
        }

        [JsonIgnore]
        public string QuestionText
        {
            get { return template.QuestionText; }
            set { throw new InvalidOperationException("question text can't be changed at binded question");}
        }
        [JsonIgnore]
        public QuestionType QuestionType
        {
            get { return template.QuestionType; }
            set { throw new InvalidOperationException("QuestionType can't be changed at binded question"); }
        }
        [JsonIgnore]
        public List<IAnswer> Answers
        {
            get { return template.Answers; }
            set { throw new InvalidOperationException("Answers can't be changed at binded question"); }
        }
        [JsonIgnore]
        public string ConditionExpression
        {
            get { return template.ConditionExpression; }
            set { throw new InvalidOperationException("ConditionExpression can't be changed at binded question"); }
        }
        [JsonIgnore]
        public string StataExportCaption
        {
            get { return template.StataExportCaption; }
            set { throw new InvalidOperationException("StataExportCaption can't be changed at binded question"); }
        }

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
    }
}
