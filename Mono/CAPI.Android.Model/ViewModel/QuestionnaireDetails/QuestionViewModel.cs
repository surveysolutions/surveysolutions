using System;
using System.Collections.Generic;
using Main.Core.Entities.SubEntities;
using Newtonsoft.Json.Linq;
using WB.Core.SharedKernels.DataCollection.DataTransferObjects.Synchronization;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;

namespace CAPI.Android.Core.Model.ViewModel.QuestionnaireDetails
{
    public abstract class QuestionViewModel : Cirrious.MvvmCross.ViewModels.MvxViewModel, IQuestionnaireItemViewModel
    {
        protected QuestionViewModel(
            InterviewItemId publicKey,
            string text,
            QuestionType questionType,
            bool enabled,
            string instructions,
            string comments,
            bool valid,
            bool mandatory,
            object answerObject,
            string validationMessage)
        {
            PublicKey = publicKey;
            ValidationMessage = validationMessage;
            Text = text;
            QuestionType = questionType;
            AnswerObject = answerObject;
            Mandatory = mandatory;
            Instructions = instructions;
            Comments = comments;

            Status = Status | QuestionStatus.ParentEnabled;
            if (enabled)
            {
                Status = Status | QuestionStatus.Enabled;
            }
            if (valid)
            {
                Status = Status | QuestionStatus.Valid;
            }
            var answered = answerObject != null;
            if (answered)
                Status = Status | QuestionStatus.Answered;
        }

        public InterviewItemId PublicKey { get; private set; }
        public string Text { get; private set; }
        public QuestionType QuestionType { get; private set; }
        public string Instructions { get; private set; }
        public string Comments { get; private set; }

        public virtual string AnswerString
        {
            get { return (AnswerObject ?? "").ToString(); }
        }

        public object AnswerObject { get; private set; }
        public bool Mandatory { get; private set; }
        public QuestionStatus Status { get; protected set; }
        public string ValidationMessage { get; private set; }

        public abstract IQuestionnaireItemViewModel Clone(int[] propagationVector);

        public virtual void SetAnswer(object answer)
        {
            if (answer == null)
            {
                return;
            }

            this.AnswerObject = answer;
            if (!Status.HasFlag(QuestionStatus.Answered))
            {
                Status = Status | QuestionStatus.Answered;
                RaisePropertyChanged("Status");
            }
            RaisePropertyChanged("AnswerString");
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

        public bool IsEnabled()
        {
            return this.Status.HasFlag(QuestionStatus.Enabled) && this.Status.HasFlag(QuestionStatus.ParentEnabled);
        }

        public void SetParentEnabled(bool enabled)
        {
            if (Status.HasFlag(QuestionStatus.ParentEnabled) == enabled)
                return;
            if (enabled)
                Status = Status | QuestionStatus.ParentEnabled;
            else
                Status &= ~QuestionStatus.ParentEnabled;
            RaisePropertyChanged("Status");
        }

        protected T[] GetValueFromJArray<T>(object answer)
        {
            try
            {
                return ((JArray)answer).ToObject<T[]>();
            }
            catch (Exception)
            {
                return new T[0];
            }
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