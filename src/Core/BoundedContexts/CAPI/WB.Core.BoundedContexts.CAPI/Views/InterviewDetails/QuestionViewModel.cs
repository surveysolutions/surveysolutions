using System;
using System.Collections.Generic;
using System.Linq;
using Main.Core.Entities.SubEntities;
using Microsoft.Practices.ServiceLocation;
using WB.Core.SharedKernels.DataCollection.ValueObjects;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Core.SharedKernels.ExpressionProcessor.Services;

namespace WB.Core.BoundedContexts.Capi.Views.InterviewDetails
{
    public abstract class QuestionViewModel : Cirrious.MvvmCross.ViewModels.MvxViewModel, IQuestionnaireItemViewModel
    {
        private const string mandatoryValidationMessage = "This question cannot be empty";

        protected QuestionViewModel(
            InterviewItemId publicKey,
            ValueVector<Guid> questionRosterScope,
            string text,
            QuestionType questionType,
            bool enabled,
            string instructions,
            string comments,
            bool valid,
            bool mandatory,
            object answerObject,
            string validationMessage,
            string variable,
            IEnumerable<string> substitutionReferences)
        {
            this.PublicKey = publicKey;
            this.QuestionRosterScope = questionRosterScope;
            this.ValidationMessage = validationMessage;
            this.SubstitutionReferences = substitutionReferences;
            this.referencedQuestionAnswers = this.SubstitutionReferences.ToDictionary(x => x,
                y => this.SubstitutionService.DefaultSubstitutionText);
            this.SourceText = this.Text = text;

            this.ReplaceSubstitutionVariables();

            this.QuestionType = questionType;
            this.AnswerObject = answerObject;
            this.Mandatory = mandatory;
            this.Instructions = instructions;
            this.Comments = comments;
            this.Variable = variable;

            this.Status = this.Status | QuestionStatus.ParentEnabled;
            if (enabled)
            {
                this.Status = this.Status | QuestionStatus.Enabled;
            }
            if (valid)
            {
                this.Status = this.Status | QuestionStatus.Valid;
            }
            var answered = answerObject != null;
            if (answered)
                this.Status = this.Status | QuestionStatus.Answered;
        }

        public InterviewItemId PublicKey { get; private set; }
        public ValueVector<Guid> QuestionRosterScope { get; private set; }
        public string SourceText { get; private set; }
        public string Text { get; private set; }
        public QuestionType QuestionType { get; private set; }
        public string Instructions { get; private set; }
        public string Comments { get; private set; }
        public string Variable { get; private set; }
        public IEnumerable<string> SubstitutionReferences { get; private set; }
        private readonly Dictionary<string, string> referencedQuestionAnswers = new Dictionary<string, string>();

        public virtual string AnswerString
        {
            get { return (this.AnswerObject ?? "").ToString(); }
        }

        public object AnswerObject { get; private set; }
        public bool Mandatory { get; private set; }
        public QuestionStatus Status { get; protected set; }


        public bool IsMandatoryAndEmpty
        {
            get
            {
                return this.Mandatory &&
                    String.IsNullOrEmpty(this.AnswerString) &&
                    !this.Status.HasFlag(QuestionStatus.Valid) &&
                    this.Status.HasFlag(QuestionStatus.Enabled) &&
                    Status.HasFlag(QuestionStatus.ParentEnabled);
            }
        }

        public string MandatoryValidationMessage
        {
            get { return mandatoryValidationMessage; }
        }
        
        public string ValidationMessage { get; private set; }

        public abstract IQuestionnaireItemViewModel Clone(decimal[] propagationVector);

        public virtual void SetAnswer(object answer)
        {
            if (answer == null)
            {
                return;
            }

            this.AnswerObject = answer;

            if (!this.Status.HasFlag(QuestionStatus.Answered))
            {
                this.Status = this.Status | QuestionStatus.Answered;
                this.RaisePropertyChanged("Status");
            }

            this.RaisePropertyChanged("AnswerString");
            this.RaisePropertyChanged("IsMandatoryAndEmpty");
        }

        public virtual void SubstituteQuestionText(QuestionViewModel referencedQuestion)
        {
            this.referencedQuestionAnswers[referencedQuestion.Variable] = string.IsNullOrEmpty(referencedQuestion.AnswerString)
                ? this.SubstitutionService.DefaultSubstitutionText
                : referencedQuestion.AnswerString;

            this.ReplaceSubstitutionVariables();

            this.RaisePropertyChanged(() => this.Text);
        }

        public virtual void SubstituteRosterTitle(string rosterTitle)
        {
            this.referencedQuestionAnswers[this.SubstitutionService.RosterTitleSubstitutionReference] = string.IsNullOrEmpty(rosterTitle)
                ? this.SubstitutionService.DefaultSubstitutionText
                : rosterTitle;

            this.ReplaceSubstitutionVariables();

            this.RaisePropertyChanged(() => this.Text);
        }

        private void ReplaceSubstitutionVariables()
        {
            this.Text = this.SourceText;
            foreach (var substitutionReference in this.SubstitutionReferences)
            {
                this.Text = this.SubstitutionService.ReplaceSubstitutionVariable(this.Text, substitutionReference,
                    this.referencedQuestionAnswers[substitutionReference]);
            }
        }

        public virtual void RemoveAnswer()
        {
            this.AnswerObject = null;

            if (this.Status.HasFlag(QuestionStatus.Answered))
            {
                this.Status ^= QuestionStatus.Answered;
                this.RaisePropertyChanged("Status");
            }

            this.RaisePropertyChanged("AnswerString");
            this.RaisePropertyChanged("IsMandatoryAndEmpty");
        }

        public virtual void SetComment(string comment)
        {
            this.Comments = comment;
            this.RaisePropertyChanged("Comments");
        }

        public virtual void SetEnabled(bool enabled)
        {
            if (this.Status.HasFlag(QuestionStatus.Enabled) == enabled)
                return;
            if (enabled)
                this.Status = this.Status | QuestionStatus.Enabled;
            else
                this.Status &= ~QuestionStatus.Enabled;
            this.RaisePropertyChanged("Status");
            this.RaisePropertyChanged("IsMandatoryAndEmpty");
        }

        public virtual void SetValid(bool valid)
        {
            if (this.Status.HasFlag(QuestionStatus.Valid) == valid)
                return;
            if (valid)
                this.Status = this.Status | QuestionStatus.Valid;
            else
                this.Status &= ~QuestionStatus.Valid;
            this.RaisePropertyChanged("Status");
            this.RaisePropertyChanged("IsMandatoryAndEmpty");
        }

        public bool IsEnabled()
        {
            return this.Status.HasFlag(QuestionStatus.Enabled) && this.Status.HasFlag(QuestionStatus.ParentEnabled);
        }

        public void SetParentEnabled(bool enabled)
        {
            if (this.Status.HasFlag(QuestionStatus.ParentEnabled) == enabled)
                return;
            if (enabled)
                this.Status = this.Status | QuestionStatus.ParentEnabled;
            else
                this.Status &= ~QuestionStatus.ParentEnabled;
            this.RaisePropertyChanged("Status");
            this.RaisePropertyChanged("IsMandatoryAndEmpty");
        }

        protected ISubstitutionService SubstitutionService
        {
            get { return ServiceLocator.Current.GetInstance<ISubstitutionService>(); }
        }
    }

    [Flags]
    public enum QuestionStatus
    {
        None = 0,
        Enabled = 1,
        Valid = 2,
        Answered = 4,
        ParentEnabled = 8
    }
 }