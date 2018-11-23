using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using Main.Core.Entities.Composite;
using WB.Core.SharedKernels.NonConficltingNamespace;
using WB.Core.SharedKernels.QuestionnaireEntities;

// ReSharper disable once CheckNamespace
namespace Main.Core.Entities.SubEntities
{
    [DebuggerDisplay("Question {PublicKey}")]
    public abstract class AbstractQuestion : IQuestion
    {
        protected AbstractQuestion(string questionText = null, List<IComposite> children = null)
        {
            this.validationConditions = new List<ValidationCondition>();
            this.Properties = new QuestionProperties(false, false);
            this.QuestionText = questionText;
        }

        public Order? AnswerOrder { get; set; }

        public List<Answer> Answers { get; set; } = new List<Answer>();

        private ReadOnlyCollection<IComposite> children = new ReadOnlyCollection<IComposite>(new List<IComposite>(0));

        public ReadOnlyCollection<IComposite> Children
        {
            get
            {
                return children;
            }
            set
            {
                // do nothing
            }
        }

        public string Comments { get; set; }

        public string ConditionExpression { get; set; }

        public bool HideIfDisabled { get; set; }

        public bool Featured { get; set; }

        public string Instructions { get; set; }

        public QuestionProperties Properties { get; set; }

        private IComposite parent;
        private IList<ValidationCondition> validationConditions;

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

        public string LinkedFilterExpression { get; set; }

        public bool? IsFilteredCombobox { get; set; }

        public bool IsTimestamp { get; set; }

        public IList<ValidationCondition> ValidationConditions
        {
            get
            {
                return this.validationConditions.ConcatWithOldConditionIfNotEmpty(ValidationExpression, ValidationMessage);
            }
            set
            {
                if (value != null)
                {
                    this.validationConditions = value;
                }
            }
        }

        public abstract void AddAnswer(Answer answer);

        public virtual IComposite Clone()
        {
            var question = this.MemberwiseClone() as IQuestion;

            question.SetParent(null);

            // handle reference part
            question.Answers = new List<Answer>();
            foreach (var answer in this.Answers ?? new List<Answer>())
            {
                question.Answers.Add(answer.Clone());
            }

            question.ValidationConditions = new List<ValidationCondition>(this.ValidationConditions.Select(x => x.Clone()));
            question.Properties = this.Properties;

            return question;
        }

        public void Insert(int index, IComposite itemToInsert, Guid? parent)
        {
        }

        public void RemoveChild(Guid child)
        {
            
        }

        public string VariableName => this.StataExportCaption;

        public void ConnectChildrenWithParent()
        {
        }

        public abstract T Find<T>(Guid publicKey) where T : class, IComposite;

        public abstract IEnumerable<T> Find<T>(Func<T, bool> condition) where T : class;

        public abstract T FirstOrDefault<T>(Func<T, bool> condition) where T : class;

        public override string ToString()
        {
            return String.Format("Question {{{0}}} '{1}'", this.PublicKey, this.QuestionText ?? "<untitled>");
        }
    }
}
