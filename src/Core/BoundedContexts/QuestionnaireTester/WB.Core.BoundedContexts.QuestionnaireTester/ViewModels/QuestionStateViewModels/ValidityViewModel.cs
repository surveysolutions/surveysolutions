using System;
using System.Linq;
using Cirrious.MvvmCross.ViewModels;
using WB.Core.BoundedContexts.QuestionnaireTester.Implementation.Entities;
using WB.Core.BoundedContexts.QuestionnaireTester.Properties;
using WB.Core.BoundedContexts.QuestionnaireTester.Repositories;
using WB.Core.BoundedContexts.QuestionnaireTester.ViewModels.QuestionsViewModels;
using WB.Core.Infrastructure.EventBus.Lite;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Exceptions;


namespace WB.Core.BoundedContexts.QuestionnaireTester.ViewModels.QuestionStateViewModels
{
    public class ValidityViewModel : MvxNotifyPropertyChanged,
        IInterviewEntityViewModel,
        ILiteEventHandler<AnswersDeclaredValid>,
        ILiteEventHandler<AnswersDeclaredInvalid>
    {
        public class ErrorMessage
        {
            public ErrorMessage(string caption, string message)
            {
                Message = message;
                Caption = caption;
            }

            public string Message { get; private set; }

            public string Caption { get; private set; }
        }

        private readonly ILiteEventRegistry liteEventRegistry;
        private readonly IStatefulInterviewRepository interviewRepository;
        private readonly IPlainKeyValueStorage<QuestionnaireModel> plainQuestionnaireRepository;

        protected ValidityViewModel() { }

        public ValidityViewModel(ILiteEventRegistry liteEventRegistry,
            IStatefulInterviewRepository interviewRepository,
            IPlainKeyValueStorage<QuestionnaireModel> plainQuestionnaireRepository)
        {
            this.liteEventRegistry = liteEventRegistry;
            this.interviewRepository = interviewRepository;
            this.plainQuestionnaireRepository = plainQuestionnaireRepository;
        }

        private string interviewId;
        private Identity questionIdentity;
        private SharedKernels.DataCollection.Events.Interview.Dtos.Identity identityForEvents;

        public void Init(string interviewId, Identity entityIdentity, NavigationState navigationState)
        {
            if (entityIdentity == null) throw new ArgumentNullException("entityIdentity");

            this.interviewId = interviewId;
            this.questionIdentity = entityIdentity;
            this.identityForEvents = entityIdentity.ToIdentityForEvents();

            liteEventRegistry.Subscribe(this);

            this.UpdateValidState();
        }

        private ErrorMessage errorFromViewModel;

        private bool isInvalid;
        public bool IsInvalid
        {
            get { return isInvalid; }
            private set { isInvalid = value; RaisePropertyChanged(); }
        }

        private ErrorMessage error;
        public ErrorMessage Error
        {
            get { return this.error; }
            private set { this.error = value; RaisePropertyChanged(); }
        }

        private void UpdateValidState()
        {
            var interview = this.interviewRepository.Get(this.interviewId);

            bool isInvalidAnswer = !interview.IsValid(this.questionIdentity);
            bool wasError = this.errorFromViewModel != null;

            if (isInvalidAnswer)
            {
                var questionnaireModel = plainQuestionnaireRepository.GetById(interview.QuestionnaireId);
                var questionModel = questionnaireModel.Questions[questionIdentity.Id];
                this.Error = new ErrorMessage(
                    UIResources.Validity_Answered_Invalid_ErrorCaption, 
                    questionModel.ValidationMessage);
            }
            else if (wasError)
            {
                this.Error = this.errorFromViewModel;
            }

            this.IsInvalid = isInvalidAnswer || wasError;
        }


        public void Handle(AnswersDeclaredValid @event)
        {
            if (@event.Questions.Contains(identityForEvents))
            {
                UpdateValidState();
            }
        }

        public void Handle(AnswersDeclaredInvalid @event)
        {
            if (@event.Questions.Contains(identityForEvents))
            {
                UpdateValidState();
            }
        }

        public virtual void ProcessException(Exception exception)
        {
            if (exception is InterviewException)
            {
                this.errorFromViewModel = new ErrorMessage(
                    UIResources.Validity_InterviewException_ErrorCaption,
                    exception.Message);

                UpdateValidState();
            }
        }

        public virtual void ExecutedWithoutExceptions()
        {
            this.errorFromViewModel = null;

            UpdateValidState();
        }

        public virtual void MarkAnswerAsInvalidWithMessage(string errorMessageText)
        {
            this.errorFromViewModel = new ErrorMessage(
                string.Empty,
                errorMessageText);

            UpdateValidState();
        }
    }
}