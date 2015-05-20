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
        private readonly ILiteEventRegistry liteEventRegistry;
        private readonly IStatefullInterviewRepository interviewRepository;
        private readonly IPlainKeyValueStorage<QuestionnaireModel> plainQuestionnaireRepository;

        public ValidityViewModel(ILiteEventRegistry liteEventRegistry,
            IStatefullInterviewRepository interviewRepository,
            IPlainKeyValueStorage<QuestionnaireModel> plainQuestionnaireRepository)
        {
            this.liteEventRegistry = liteEventRegistry;
            this.interviewRepository = interviewRepository;
            this.plainQuestionnaireRepository = plainQuestionnaireRepository;
        }

        private string interviewId;
        private Identity entityIdentity;
        private SharedKernels.DataCollection.Events.Interview.Dtos.Identity identityForEvents;

        public void Init(string interviewId, Identity entityIdentity, NavigationState navigationState)
        {
            if (entityIdentity == null) throw new ArgumentNullException("entityIdentity");

            this.interviewId = interviewId;
            this.entityIdentity = entityIdentity;
            this.identityForEvents = entityIdentity.ToIdentityForEvents();

            liteEventRegistry.Subscribe(this);

            this.UpdateValidState();
        }

        private Exception exception;

        private bool isInvalid;
        public bool IsInvalid
        {
            get { return isInvalid; }
            private set { isInvalid = value; RaisePropertyChanged(); }
        }        
        
        private string errorMessage;
        public string ErrorMessage
        {
            get { return errorMessage; }
            private set { errorMessage = value; RaisePropertyChanged(); }
        }        
        
        private string errorCaption;
        public string ErrorCaption
        {
            get { return errorCaption; }
            private set { errorCaption = value; RaisePropertyChanged(); }
        }

        private void UpdateValidState()
        {
            var interview = this.interviewRepository.Get(this.interviewId);

            bool isInvalidAnswer = !interview.IsValid(this.entityIdentity);
            bool wasException = exception != null;
            string errorMessageText = String.Empty;
            string errorCaptionText = String.Empty;

            if (isInvalidAnswer)
            {
                var questionnaireModel = plainQuestionnaireRepository.GetById(interview.QuestionnaireId);
                var questionModel = questionnaireModel.Questions[entityIdentity.Id];
                errorMessageText = questionModel.ValidationMessage;
                errorCaptionText = UIResources.Validity_Answered_Invalid_ErrorCaption;
            }
            else if (wasException && exception is InterviewException)
            {
                errorCaptionText = UIResources.Validity_ApplicationException_ErrorCaption;
                errorMessageText = UIResources.Validity_ApplicationException_ErrorMessage;
            }

            IsInvalid = isInvalidAnswer || wasException;
            ErrorMessage = errorMessageText;
            ErrorCaption = errorCaptionText;
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

        public void ProcessException(Exception ex)
        {
            this.exception = ex;

            UpdateValidState();
        }

        public void ExecutedWithoutExceptions()
        {
            exception = null;

            UpdateValidState();
        }
    }
}