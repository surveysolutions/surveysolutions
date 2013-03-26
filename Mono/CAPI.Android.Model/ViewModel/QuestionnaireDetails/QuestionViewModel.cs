using System;
using System.Collections.Generic;
using Main.Core.Entities.SubEntities;

namespace CAPI.Android.Core.Model.ViewModel.QuestionnaireDetails
{


    public abstract class QuestionViewModel : Cirrious.MvvmCross.ViewModels.MvxViewModel, IQuestionnaireItemViewModel
    {
        protected QuestionViewModel(
            ItemPublicKey publicKey,
            string text,
            QuestionType questionType,
            bool enabled,
            string instructions,
            string comments,
            bool valid,
            bool mandatory,
            bool capital,
            string answerString,
            string validationExpression,
            string validationMessage)
        {
            PublicKey = publicKey;
            ValidationExpression = validationExpression;
            ValidationMessage = validationMessage;
            Text = text;
            QuestionType = questionType;
            AnswerString = answerString;
            Capital = capital;
            Mandatory = mandatory;
            Instructions = instructions;
            Comments = comments;

            if (enabled)
            {
                Status = Status | QuestionStatus.Enabled;
            }
            if (valid)
            {
                Status = Status | QuestionStatus.Valid;
            }
            var answered = !string.IsNullOrEmpty(answerString);
            if (answered)
                Status = Status | QuestionStatus.Answered;
            
        }
        protected QuestionViewModel(
           ItemPublicKey publicKey,
           string text,
           QuestionType questionType,
           QuestionStatus status,
           string instructions,
           string comments,
           bool mandatory,
           bool capital,
           string answerString,
           string validationExpression,
           string validationMessage)
        {
            PublicKey = publicKey;
            ValidationExpression = validationExpression;
            ValidationMessage = validationMessage;
            Text = text;
            QuestionType = questionType;
            AnswerString = answerString;
            Capital = capital;
            Instructions = instructions;
            Comments = comments;
            Mandatory = mandatory;

            Status = status;
        }
        public ItemPublicKey PublicKey { get; private set; }
        public string Text { get; private set; }
        public QuestionType QuestionType { get; private set; }
        public bool Capital { get; private set; }
        public string Instructions { get; private set; }
        public string Comments { get; private set; }
        public string AnswerString { get; protected set; }
        public abstract string AnswerObject { get; }
        public bool Mandatory { get; private set; }
        public QuestionStatus Status { get; protected set; }
        public string ValidationExpression { get; private set; }
        public string ValidationMessage { get; private set; }

        public abstract IQuestionnaireItemViewModel Clone(Guid propagationKey);

        public virtual void SetAnswer(List<Guid> answer, string answerString)
        {
            this.AnswerString = answerString;
            if (!Status.HasFlag(QuestionStatus.Answered))
            {
                Status = Status | QuestionStatus.Answered;
                RaisePropertyChanged("Status");
            }
            RaisePropertyChanged("AnswerString");
        }

        public virtual void SetComment(string comment)
        {
            this.Comments = comment;
            RaisePropertyChanged("Comments");
        }

        public virtual void SetEnabled(bool enabled)
        {
            if (Status.HasFlag(QuestionStatus.Enabled) == enabled)
                return;
            if (enabled)
                Status = Status | QuestionStatus.Enabled;
            else
                Status &= ~QuestionStatus.Enabled;
            RaisePropertyChanged("Status");
        }

        public virtual void SetValid(bool valid)
        {
            if (Status.HasFlag(QuestionStatus.Valid) == valid)
                return;
            if (valid)
                Status = Status | QuestionStatus.Valid;
            else
                Status &= ~QuestionStatus.Valid;
            RaisePropertyChanged("Status");
        }
    }

    [Flags]
    public enum QuestionStatus
    {
        None=0,
        Enabled=1,
        Valid=2,
        Answered=4
    }
}