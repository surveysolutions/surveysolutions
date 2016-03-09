using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using MvvmCross.Core.ViewModels;
using MvvmCross.Platform.Core;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.EventBus.Lite;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Exceptions;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.Enumerator.Aggregates;
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
        public class ErrorMessage : MvxNotifyPropertyChanged
        {
            private string caption;

            public ErrorMessage(string caption, IEnumerable<string> messages)
            {
                this.ValidationErrors = new ObservableCollection<string>(messages);
                this.caption = caption;
            }

            public ObservableCollection<string> ValidationErrors { get; private set; }

            public string Caption
            {
                get { return this.caption; }
                set
                {
                    this.caption = value;
                    this.RaisePropertyChanged();
                }
            }
        }

        private readonly ILiteEventRegistry liteEventRegistry;
        private readonly IStatefulInterviewRepository interviewRepository;
        private readonly IPlainQuestionnaireRepository questionnaireRepository;
        private readonly IMvxMainThreadDispatcher mainThreadDispatcher;

        protected ValidityViewModel() { }

        public ValidityViewModel(ILiteEventRegistry liteEventRegistry,
            IStatefulInterviewRepository interviewRepository,
            IPlainQuestionnaireRepository questionnaireRepository,
            IMvxMainThreadDispatcher mainThreadDispatcher)
        {
            this.liteEventRegistry = liteEventRegistry;
            this.interviewRepository = interviewRepository;
            this.questionnaireRepository = questionnaireRepository;
            this.mainThreadDispatcher = mainThreadDispatcher;
            this.error = new ErrorMessage(null, new List<string>());
        }

        private string interviewId;

        private Identity questionIdentity;

        public void Init(string interviewId, Identity entityIdentity, NavigationState navigationState)
        {
            if (entityIdentity == null) throw new ArgumentNullException(nameof(entityIdentity));

            this.interviewId = interviewId;
            this.questionIdentity = entityIdentity;

            this.liteEventRegistry.Subscribe(this, interviewId);

            this.UpdateValidState();
        }

        private string exceptionErrorMessageFromViewModel;

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
            bool wasError = this.exceptionErrorMessageFromViewModel != null;

            mainThreadDispatcher.RequestMainThreadAction(() =>
            {
                if (isInvalidAnswer && !wasError)
                {
                    var validationMessages = this.GetValidationMessages(interview).ToList();

                    this.Error.Caption = UIResources.Validity_Answered_Invalid_ErrorCaption;
                    this.Error.ValidationErrors.Clear();
                    validationMessages.ForEach(this.Error.ValidationErrors.Add);
                }
                else if (wasError)
                {
                    this.Error.Caption = UIResources.Validity_NotAnswered_InterviewException_ErrorCaption;
                    this.Error.ValidationErrors.Clear();
                    this.Error.ValidationErrors.Add(this.exceptionErrorMessageFromViewModel);
                }

                this.IsInvalid = isInvalidAnswer || wasError;
            });
        }

        private IEnumerable<string> GetValidationMessages(IStatefulInterview interview)
        {
            IReadOnlyList<FailedValidationCondition> failedConditions = interview.GetFailedValidationConditions(this.questionIdentity);
            var questionnaire = this.questionnaireRepository.GetQuestionnaire(interview.QuestionnaireIdentity);
            var validationMessages = failedConditions.Select(x =>
            {
                var validationMessage = questionnaire.GetValidationMessage(this.questionIdentity.Id, x.FailedConditionIndex);
                if (questionnaire.HasMoreThanOneValidationRule(this.questionIdentity.Id))
                {
                    validationMessage += $" [{x.FailedConditionIndex + 1}]";
                }
                return validationMessage;
            });
            return validationMessages;
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
                this.exceptionErrorMessageFromViewModel = exception.Message;

                this.UpdateValidState();
            }
        }

        public virtual void ExecutedWithoutExceptions()
        {
            this.exceptionErrorMessageFromViewModel = null;

            this.UpdateValidState();
        }

        public virtual void MarkAnswerAsNotSavedWithMessage(string errorMessageText)
        {
            this.exceptionErrorMessageFromViewModel = errorMessageText;

            this.UpdateValidState();
        }

        public void Dispose()
        {
            this.liteEventRegistry.Unsubscribe(this, this.interviewId);
        }
    }
}