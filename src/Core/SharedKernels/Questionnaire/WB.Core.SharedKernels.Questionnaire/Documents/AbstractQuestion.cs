using System;
using System.Collections.Generic;
using System.Diagnostics;
using Main.Core.Entities.Composite;
using WB.Core.GenericSubdomains.Portable.Implementation.Services;

// ReSharper disable once CheckNamespace
namespace Main.Core.Entities.SubEntities
{
    [DebuggerDisplay("Question {PublicKey}")]
    public abstract class AbstractQuestion : IQuestion
    {
        protected AbstractQuestion()
        {
            this.Answers = new List<Answer>();
        }

        protected AbstractQuestion(string text)
            : this()
        {
            this.QuestionText = text;
        }

        public Order? AnswerOrder { get; set; }

        public List<Answer> Answers { get; set; }

        public bool Capital { get; set; }

        public List<IComposite> Children
        {
            get
            {
                return new List<IComposite>(0);
            }
            set
            {
                // do nothing
            }
        }

        public string Comments { get; set; }

        public string ConditionExpression { get; set; }

        public bool Featured { get; set; }

        public string Instructions { get; set; }

        
        private IComposite parent;

        public IComposite GetParent()
        {
            return this.parent;
        }

        public void SetParent(IComposite parent)
        {
            this.parent = parent;
        }

        public Guid PublicKey { get; set; }

        public QuestionScope QuestionScope { get; set; }

        public string QuestionText { get; set; }

        public virtual QuestionType QuestionType { get; set; }

        public string StataExportCaption { get; set; }

        public string VariableLabel { get; set; }

        public string ValidationExpression { get; set; }

        public string ValidationMessage { get; set; }

        public Guid? LinkedToRosterId { get; set; }

        /// <summary>
        /// Id of parent question to cascade from 
        /// </summary>
        public Guid? CascadeFromQuestionId { get; set; }

        public Guid? LinkedToQuestionId { get; set; }
        
        public bool? IsFilteredCombobox { get; set; }
        
        public abstract void AddAnswer(Answer answer);

        public virtual IComposite Clone()
        {
            var question = this.MemberwiseClone() as IQuestion;

            question.SetParent(null);

            // handle reference part
            question.Answers = new List<Answer>();
            foreach (var answer in this.Answers)
            {
                question.Answers.Add(answer.Clone());
            }

            return question;
        }

        public void ConnectChildrenWithParent()
        {
        }

        public abstract T Find<T>(Guid publicKey) where T : class, IComposite;

        public abstract IEnumerable<T> Find<T>(Func<T, bool> condition) where T : class;

        public abstract T FirstOrDefault<T>(Func<T, bool> condition) where T : class;

        public IEnumerable<string> GetVariablesUsedInTitle()
        {
#warning: Slava: make nice injection here
            return (new SubstitutionService()).GetAllSubstitutionVariableNames(QuestionText);
        }

        public override string ToString()
        {
            return String.Format("Question {{{0}}} '{1}'", this.PublicKey, this.QuestionText ?? "<untitled>");
        }
    }
}