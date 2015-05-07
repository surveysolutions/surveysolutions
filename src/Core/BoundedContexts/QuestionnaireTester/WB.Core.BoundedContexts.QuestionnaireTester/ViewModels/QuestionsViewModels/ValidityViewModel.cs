using System;
using System.Linq;
using Cirrious.MvvmCross.ViewModels;
using WB.Core.GenericSubdomains.Utils;
using WB.Core.Infrastructure.EventBus.Lite;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Events.Interview;

using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities.QuestionModels;
using WB.Core.SharedKernels.DataCollection.Repositories;

namespace WB.Core.BoundedContexts.QuestionnaireTester.ViewModels.QuestionsViewModels
{
    public class ValidityViewModel : MvxNotifyPropertyChanged,
        IInterviewEntityViewModel,
        ILiteEventBusEventHandler<AnswersDeclaredValid>,
        ILiteEventBusEventHandler<AnswersDeclaredInvalid>,
        IDisposable
    {
        private readonly ILiteEventRegistry liteEventRegistry;
        private readonly IStatefulInterviewRepository interviewRepository;
        private readonly IPlainKeyValueStorage<QuestionnaireModel> plainQuestionnaireRepository;

        public ValidityViewModel(ILiteEventRegistry liteEventRegistry,
            IStatefulInterviewRepository interviewRepository,
            IPlainKeyValueStorage<QuestionnaireModel> plainQuestionnaireRepository)
        {
            this.liteEventRegistry = liteEventRegistry;
            this.interviewRepository = interviewRepository;
            this.plainQuestionnaireRepository = plainQuestionnaireRepository;
        }

        private string interviewId;
        private Identity entityIdentity;
        private SharedKernels.DataCollection.Events.Interview.Dtos.Identity identityForEvents;

        public void Init(string interviewId, Identity entityIdentity)
        {
            if (entityIdentity == null) throw new ArgumentNullException("entityIdentity");

            this.interviewId = interviewId;
            this.entityIdentity = entityIdentity;
            this.identityForEvents = entityIdentity.ToIdentityForEvents();

            liteEventRegistry.Subscribe(this);

            this.UpdateSelfFromModel();
        }

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

        private void UpdateSelfFromModel()
        {
            var interview = this.interviewRepository.Get(this.interviewId);

            this.IsInvalid = !interview.IsValid(this.entityIdentity);

            if (IsInvalid)
            {
                var questionnaireModel = plainQuestionnaireRepository.GetById(interview.QuestionnaireId);
                var questionModel = questionnaireModel.Questions[entityIdentity.Id];
                ErrorMessage = questionModel.ValidationMessage;
            }
            else
            {
                ErrorMessage = string.Empty;
            }
        }


        public void Handle(AnswersDeclaredValid @event)
        {
            if (@event.Questions.Contains(identityForEvents))
            {
                UpdateSelfFromModel();
            }
        }

        public void Handle(AnswersDeclaredInvalid @event)
        {
            if (@event.Questions.Contains(identityForEvents))
            {
                UpdateSelfFromModel();
            }
        }

        public void MarkAsError()
        {
            IsInvalid = true;
            ErrorMessage = "You've entered invalid answer.";
        }

        public void Dispose()
        {
            liteEventRegistry.Unsubscribe(this);
        }
    }
}