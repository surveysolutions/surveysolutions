using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Main.Core.Entities.Composite;
using Microsoft.Practices.ServiceLocation;
using WB.Core.SharedKernels.ExpressionProcessor.Services;

namespace Main.Core.Entities.SubEntities
{
    [DebuggerDisplay("Question {PublicKey}")]
    public abstract class AbstractQuestion : IQuestion
    {
        protected AbstractQuestion()
        {
            this.Cards = new List<Image>();
            this.Answers = new List<Answer>();
        }

        protected AbstractQuestion(string text)
            : this()
        {
            this.QuestionText = text;
        }

        public Order AnswerOrder { get; set; }

        public List<Answer> Answers { get; set; }

        public bool Capital { get; set; }

        public List<Image> Cards { get; set; }

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

        public List<Guid> ConditionalDependentQuestions { get; set; }
        public List<Guid> ConditionalDependentGroups { get; set; }
        public List<Guid> QuestionsWhichCustomValidationDependsOnQuestion { get; set; }

        public List<Guid> QuestionIdsInvolvedInCustomEnablementConditionOfQuestion
        {
            get
            {
                if (questionIdsInvolvedInCustomEnablementConditionOfQuestion == null &&
                    QuestionsInvolvedInCustomEnablementConditionOfQuestion != null)
                {
                    questionIdsInvolvedInCustomEnablementConditionOfQuestion =
                        QuestionsInvolvedInCustomEnablementConditionOfQuestion.Select(q => q.Id).ToList();
                }
                return questionIdsInvolvedInCustomEnablementConditionOfQuestion;
            }
            set { questionIdsInvolvedInCustomEnablementConditionOfQuestion = value; }
        }

        private List<Guid> questionIdsInvolvedInCustomEnablementConditionOfQuestion;

        public List<Guid> QuestionIdsInvolvedInCustomValidationOfQuestion
        {
            get
            {
                if (questionIdsInvolvedInCustomValidationOfQuestion == null && QuestionsInvolvedInCustomValidationOfQuestion != null)
                {
                    questionIdsInvolvedInCustomValidationOfQuestion =
                        QuestionsInvolvedInCustomValidationOfQuestion.Select(q => q.Id).ToList();
                }
                return questionIdsInvolvedInCustomValidationOfQuestion;
            }
            set { questionIdsInvolvedInCustomValidationOfQuestion = value; }
        }

        private List<Guid> questionIdsInvolvedInCustomValidationOfQuestion;


        [Obsolete("please use QuestionIdsInvolvedInCustomEnablementConditionOfQuestion instead")]
        public List<QuestionIdAndVariableName> QuestionsInvolvedInCustomEnablementConditionOfQuestion { get; set; }

        [Obsolete("please use QuestionIdsInvolvedInCustomValidationOfQuestion instead")]
        public List<QuestionIdAndVariableName> QuestionsInvolvedInCustomValidationOfQuestion { get; set; }

        public bool Featured { get; set; }

        public string Instructions { get; set; }

        public bool Mandatory { get; set; }

        private IComposite parent;
        private string questionText;

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

        public string QuestionText
        {
            get { return this.questionText; }
            set { this.questionText = System.Web.HttpUtility.HtmlDecode(value); }
        }

        public virtual QuestionType QuestionType { get; set; }

        public string StataExportCaption { get; set; }

        public string VariableLabel { get; set; }

        public string ValidationExpression { get; set; }

        public string ValidationMessage { get; set; }

        public Guid? LinkedToQuestionId { get; set; }

        public abstract void AddAnswer(Answer answer);

        public void AddCard(Image card)
        {
            if (this.Cards == null)
            {
                this.Cards = new List<Image>();
            }

            this.Cards.Add(card);
        }

        public virtual IComposite Clone()
        {
            var question = this.MemberwiseClone() as IQuestion;

            question.SetParent(null);

            if (this.Cards != null)
            {
                question.Cards = new List<Image>(this.Cards); // assuming that cards are structures 
            }

            if (this.ConditionalDependentGroups != null)
            {
                question.ConditionalDependentGroups = new List<Guid>(this.ConditionalDependentGroups);
            }

            if (this.ConditionalDependentQuestions != null)
            {
                question.ConditionalDependentQuestions = new List<Guid>(this.ConditionalDependentQuestions);
            }

            if (this.QuestionsWhichCustomValidationDependsOnQuestion != null)
            {
                question.QuestionsWhichCustomValidationDependsOnQuestion =
                    new List<Guid>(this.QuestionsWhichCustomValidationDependsOnQuestion);
            }

            if (this.QuestionIdsInvolvedInCustomEnablementConditionOfQuestion != null)
            {
                question.QuestionIdsInvolvedInCustomEnablementConditionOfQuestion =
                    new List<Guid>(this.QuestionIdsInvolvedInCustomEnablementConditionOfQuestion);
            }

            if (this.QuestionIdsInvolvedInCustomValidationOfQuestion != null)
            {
                question.QuestionIdsInvolvedInCustomValidationOfQuestion =
                    new List<Guid>(this.QuestionIdsInvolvedInCustomValidationOfQuestion);
            }

            // handle reference part
            question.Answers = new List<Answer>();
            foreach (Answer answer in this.Answers)
            {
                question.Answers.Add(answer.Clone());
            }

            return question;
        }

        public void ConnectChildrenWithParent()
        {
            //// do nothing
        }

        public abstract T Find<T>(Guid publicKey) where T : class, IComposite;

        public abstract IEnumerable<T> Find<T>(Func<T, bool> condition) where T : class;

        public abstract T FirstOrDefault<T>(Func<T, bool> condition) where T : class;

        public Image RemoveCard(Guid imageKey)
        {
            if (this.Cards == null)
            {
                this.Cards = new List<Image>();
            }

            Image card = this.Cards.Single(c => c.PublicKey == imageKey);
            this.Cards.Remove(card);
            return card;
        }

        public void UpdateCard(Guid imageKey, string title, string desc)
        {
            if (this.Cards == null)
            {
                this.Cards = new List<Image>();
            }

            Image card = this.Cards.Single(c => c.PublicKey == imageKey);

            card.Title = title;
            card.Description = desc;
        }

        public IEnumerable<string> GetVariablesUsedInTitle()
        {
            return ServiceLocator.Current.GetInstance<ISubstitutionService>().GetAllSubstitutionVariableNames(QuestionText);
        }

        public override string ToString()
        {
            return String.Format("Question {{{0}}} '{1}'", this.PublicKey, this.QuestionText ?? "<untitled>");
        }
    }
}