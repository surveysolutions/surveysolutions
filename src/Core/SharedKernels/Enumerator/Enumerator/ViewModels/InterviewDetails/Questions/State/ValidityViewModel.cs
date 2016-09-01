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
using WB.Core.SharedKernels.Enumerator.Utils;


namespace WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions.State
{
    public class ValidityViewModel : MvxNotifyPropertyChanged,
        ILiteEventHandler<AnswersDeclaredValid>,
        ILiteEventHandler<AnswersDeclaredInvalid>,
        ILiteEventHandler<StaticTextsDeclaredValid>,
        ILiteEventHandler<StaticTextsDeclaredInvalid>,
        ILiteEventHandler<QuestionsEnabled>,
        ICompositeEntity,
        IDisposable
    {
        private readonly ILiteEventRegistry liteEventRegistry;
        private readonly IStatefulInterviewRepository interviewRepository;
        private readonly IQuestionnaireStorage questionnaireRepository;
        private readonly IMvxMainThreadDispatcher mainThreadDispatcher;

        protected ValidityViewModel() { }

        public ValidityViewModel(ILiteEventRegistry liteEventRegistry,
            IStatefulInterviewRepository interviewRepository,
            IQuestionnaireStorage questionnaireRepository,
            IMvxMainThreadDispatcher mainThreadDispatcher,
            ErrorMessagesViewModel errorMessagesViewModel)
        {
            this.liteEventRegistry = liteEventRegistry;
            this.interviewRepository = interviewRepository;
            this.questionnaireRepository = questionnaireRepository;
            this.mainThreadDispatcher = mainThreadDispatcher;
            this.Error = errorMessagesViewModel;
        }

        private string interviewId;

        private Identity questionIdentity;

        public void Init(string interviewId, Identity entityIdentity)
        {
            if (entityIdentity == null) throw new ArgumentNullException(nameof(entityIdentity));

            this.interviewId = interviewId;
            this.questionIdentity = entityIdentity;

            this.Error.Init(interviewId, entityIdentity);

            this.liteEventRegistry.Subscribe(this, interviewId);
            this.UpdateValidState();
        }

        private string exceptionErrorMessageFromViewModel;

        private bool isInvalid;
        public bool IsInvalid
        {
            get { return this.isInvalid; }
            private set
            {
                this.RaiseAndSetIfChanged(ref this.isInvalid, value);

                var isShowErrors = this.children.Contains(this.Error);
                if (value && !isShowErrors)
                {
                    this.children.Add(Error);
                }
                else if (!value && isShowErrors)
                {
                    this.children.Clear();
                }
            }
        }

        public ErrorMessagesViewModel Error { get; }

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
                    this.Error.ChangeValidationErrors(validationMessages);
                }
                else if (wasError)
                {
                    this.Error.Caption = UIResources.Validity_NotAnswered_InterviewException_ErrorCaption;
                    this.Error.ChangeValidationErrors(this.exceptionErrorMessageFromViewModel.ToEnumerable());
                }

                this.IsInvalid = isInvalidAnswer || wasError;
            });
        }

        private IEnumerable<string> GetValidationMessages(IStatefulInterview interview)
        {
            IReadOnlyList<FailedValidationCondition> failedConditions = interview.GetFailedValidationConditions(this.questionIdentity);
            var questionnaire = this.questionnaireRepository.GetQuestionnaire(interview.QuestionnaireIdentity, interview.Language);
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

        public void Handle(StaticTextsDeclaredValid @event)
        {
            if (@event.StaticTexts.Contains(this.questionIdentity))
            {
                this.UpdateValidState();
            }
        }

        public void Handle(StaticTextsDeclaredInvalid @event)
        {
            if (@event.GetFailedValidationConditionsDictionary().Keys.Contains(this.questionIdentity))
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
            this.liteEventRegistry.Unsubscribe(this);
        }


        CompositeCollection<ICompositeEntity> children = new CompositeCollection<ICompositeEntity>();
        public CompositeCollection<ICompositeEntity> Children => this.children;
    }
}