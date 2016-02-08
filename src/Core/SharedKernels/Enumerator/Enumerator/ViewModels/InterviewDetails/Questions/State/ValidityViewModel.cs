using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Cirrious.MvvmCross.ViewModels;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.EventBus.Lite;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Exceptions;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.Enumerator.Properties;
using WB.Core.SharedKernels.Enumerator.Repositories;

namespace WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions.State
{
    public class ValidityViewModel : MvxNotifyPropertyChanged,
        ILiteEventHandler<AnswersDeclaredValid>,
        ILiteEventHandler<AnswersDeclaredInvalid>,
        ILiteEventHandler<QuestionsEnabled>,
        IDisposable
    {
        public class ErrorMessage
        {
            public ErrorMessage(string caption, IEnumerable<string> messages)
            {
                this.ValidationErrors = new ObservableCollection<string>(messages);
                this.Caption = caption;
            }

            public ObservableCollection<string> ValidationErrors { get; private set; }

            public string Caption { get; private set; }
        }

        private readonly ILiteEventRegistry liteEventRegistry;
        private readonly IStatefulInterviewRepository interviewRepository;
        private readonly IPlainQuestionnaireRepository questionnaireRepository;

        protected ValidityViewModel() { }

        public ValidityViewModel(ILiteEventRegistry liteEventRegistry,
            IStatefulInterviewRepository interviewRepository,
            IPlainQuestionnaireRepository questionnaireRepository)
        {
            this.liteEventRegistry = liteEventRegistry;
            this.interviewRepository = interviewRepository;
            this.questionnaireRepository = questionnaireRepository;
        }

        public void Dispose()
        {
            this.liteEventRegistry.Unsubscribe(this, interviewId);
        }

        private string interviewId;
        private Identity questionIdentity;

        public void Init(string interviewId, Identity entityIdentity, NavigationState navigationState)
        {
            if (entityIdentity == null) throw new ArgumentNullException("entityIdentity");

            this.interviewId = interviewId;
            this.questionIdentity = entityIdentity;

            this.liteEventRegistry.Subscribe(this, interviewId);

            this.UpdateValidState();
        }

        private ErrorMessage errorFromViewModel;

        private bool isInvalid;
        public bool IsInvalid
        {
            get { return this.isInvalid; }
            private set { this.isInvalid = value; this.RaisePropertyChanged(); }
        }

        private ErrorMessage error;
        public ErrorMessage Error
        {
            get { return this.error; }
            private set { this.error = value; this.RaisePropertyChanged(); }
        }

        private void UpdateValidState()
        {
            var interview = this.interviewRepository.Get(this.interviewId);

            bool isInvalidAnswer = !interview.IsValid(this.questionIdentity) && interview.WasAnswered(this.questionIdentity);
            bool wasError = this.errorFromViewModel != null;

            if (isInvalidAnswer && !wasError)
            {
                IReadOnlyList<FailedValidationCondition> failedConditions = interview.GetValidationMessages(this.questionIdentity);
                var questionnaire = this.questionnaireRepository.GetQuestionnaire(interview.QuestionnaireIdentity);
                var validationMessages = failedConditions.Select(x => questionnaire.GetValidationMessage(this.questionIdentity.Id, x.FailedConditionIndex));

                this.Error = new ErrorMessage(UIResources.Validity_Answered_Invalid_ErrorCaption, validationMessages);
            }
            else if (wasError)
            {
                this.Error = this.errorFromViewModel;
            }

            this.IsInvalid = isInvalidAnswer || wasError;
        }


        public void Handle(AnswersDeclaredValid @event)
        {
            if (@event.Questions.Contains(this.questionIdentity))
            {
                this.UpdateValidState();
            }
        }

        public void Handle(AnswersDeclaredInvalid @event)
        {
            if (@event.FailedValidationConditions.Keys.Contains(this.questionIdentity))
            {
                this.UpdateValidState();
            }
        }

        public void Handle(QuestionsEnabled @event)
        {
            if (@event.Questions.Contains(this.questionIdentity))
            {
                this.UpdateValidState();
            }
        }

        public virtual void ProcessException(Exception exception)
        {
            if (exception is InterviewException)
            {
                this.errorFromViewModel = new ErrorMessage(
                    UIResources.Validity_NotAnswered_InterviewException_ErrorCaption,
                    exception.Message.ToEnumerable());

                this.UpdateValidState();
            }
        }

        public virtual void ExecutedWithoutExceptions()
        {
            this.errorFromViewModel = null;

            this.UpdateValidState();
        }

        public virtual void MarkAnswerAsNotSavedWithMessage(string errorMessageText)
        {
            this.errorFromViewModel = new ErrorMessage(
                UIResources.Validity_NotAnswered_InterviewException_ErrorCaption,
                errorMessageText.ToEnumerable());

            this.UpdateValidState();
        }
    }
}