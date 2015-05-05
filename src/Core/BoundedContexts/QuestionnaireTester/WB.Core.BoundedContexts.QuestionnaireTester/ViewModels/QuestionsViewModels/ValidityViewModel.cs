using System;
using System.Linq;
using Cirrious.MvvmCross.ViewModels;
using WB.Core.GenericSubdomains.Utils;
using WB.Core.Infrastructure.EventBus.Lite;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Events.Interview;

using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.Repositories;

namespace WB.Core.BoundedContexts.QuestionnaireTester.ViewModels.QuestionsViewModels
{
    public class ValidityViewModel : MvxNotifyPropertyChanged,
        IInterviewEntityViewModel,
        ILiteEventBusEventHandler<AnswersDeclaredValid>,
        ILiteEventBusEventHandler<AnswersDeclaredInvalid>
    {
        private readonly IPlainRepository<InterviewModel> interviewRepository;

        public ValidityViewModel(IPlainRepository<InterviewModel> interviewRepository)
        {
            this.interviewRepository = interviewRepository;
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

            this.UpdateSelfFromModel();
        }

        private bool isValid;
        public bool IsValid
        {
            get { return isValid; }
            private set { isValid = value; RaisePropertyChanged(); }
        }

        private void UpdateSelfFromModel()
        {
            var interview = this.interviewRepository.Get(this.interviewId);

            this.IsValid = interview.IsValid(this.entityIdentity);
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
    }
}