using System;
using System.Linq;
using MvvmCross.Base;
using MvvmCross.ViewModels;
using WB.Core.GenericSubdomains.Portable.Tasks;
using WB.Core.Infrastructure.EventBus.Lite;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.Enumerator.Properties;

namespace WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions.State
{
    public class WarningsViewModel : MvxNotifyPropertyChanged,
        ILiteEventHandler<AnswersDeclaredPlausible>,
        ILiteEventHandler<AnswersDeclaredImplausible>,
        ILiteEventHandler<StaticTextsDeclaredPlausible>,
        ILiteEventHandler<StaticTextsDeclaredImplausible>,
        ILiteEventHandler<QuestionsEnabled>,
        ILiteEventHandler<SubstitutionTitlesChanged>,
        ICompositeEntity,
        IDisposable
    {
        private readonly ILiteEventRegistry liteEventRegistry;
        private readonly IStatefulInterviewRepository interviewRepository;
        private readonly IMvxMainThreadAsyncDispatcher mainThreadDispatcher;

        protected WarningsViewModel() { }

        public WarningsViewModel(ILiteEventRegistry liteEventRegistry,
            IStatefulInterviewRepository interviewRepository,
            IMvxMainThreadAsyncDispatcher mainThreadDispatcher,
            ErrorMessagesViewModel errorMessagesViewModel)
        {
            this.liteEventRegistry = liteEventRegistry;
            this.interviewRepository = interviewRepository;
            this.mainThreadDispatcher = mainThreadDispatcher;
            this.Warning = errorMessagesViewModel;
        }

        private string interviewId;

        public Identity Identity { get; set; }

        public void Init(string interviewId, Identity entityIdentity)
        {
            if (entityIdentity == null) throw new ArgumentNullException(nameof(entityIdentity));
            this.interviewId = interviewId;
            this.Identity = entityIdentity;

            this.liteEventRegistry.Subscribe(this, interviewId);
            this.UpdateValidState();
        }
        
        private bool isImplausible;
        public bool IsImplausible
        {
            get => this.isImplausible;
            private set => this.RaiseAndSetIfChanged(ref this.isImplausible, value);
        }

        public ErrorMessagesViewModel Warning { get; }

        private void UpdateValidState()
        {
            var interview = this.interviewRepository.Get(this.interviewId);

            bool isInvalidEntity = !interview.IsEntityPlausible(this.Identity);

            this.mainThreadDispatcher.ExecuteOnMainThreadAsync(() =>
            {
                if (isInvalidEntity)
                {
                    var validationMessages = interview.GetFailedWarningMessages(this.Identity, UIResources.Warning);

                    this.Warning.Caption = UIResources.Warnings;
                    this.Warning.ChangeValidationErrors(validationMessages);
                }

                this.IsImplausible = isInvalidEntity;
            }).WaitAndUnwrapException();
        }

        public void Handle(AnswersDeclaredPlausible @event)
        {
            if (@event.Questions.Contains(this.Identity))
            {
                this.UpdateValidState();
            }
        }

        public void Handle(AnswersDeclaredImplausible @event)
        {
            if (@event.GetFailedValidationConditionsDictionary().Keys.Contains(this.Identity))
            {
                this.UpdateValidState();
            }
        }

        public void Handle(StaticTextsDeclaredPlausible @event)
        {
            if (@event.StaticTexts.Contains(this.Identity))
            {
                this.UpdateValidState();
            }
        }

        public void Handle(StaticTextsDeclaredImplausible @event)
        {
            if (@event.GetFailedValidationConditionsDictionary().Keys.Contains(this.Identity))
            {
                this.UpdateValidState();
            }
        }

        public void Handle(QuestionsEnabled @event)
        {
            if (@event.Questions.Contains(this.Identity))
            {
                this.UpdateValidState();
            }
        }

        public void Handle(SubstitutionTitlesChanged @event)
        {
            if (@event.Questions.Contains(this.Identity) || @event.StaticTexts.Contains(this.Identity))
            {
                this.UpdateValidState();
            }
        }

        public void Dispose()
        {
            this.liteEventRegistry.Unsubscribe(this);
            this.Warning.Dispose();
        }
    }
}
