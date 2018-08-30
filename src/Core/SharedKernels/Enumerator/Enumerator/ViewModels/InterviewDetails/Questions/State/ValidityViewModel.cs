using System;
using System.Linq;
using System.Threading.Tasks;
using MvvmCross.Base;
using MvvmCross.ViewModels;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Portable.Tasks;
using WB.Core.Infrastructure.EventBus.Lite;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Exceptions;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.Enumerator.Properties;

namespace WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions.State
{
    public class ValidityViewModel : MvxNotifyPropertyChanged,
        ILiteEventHandler<AnswersDeclaredValid>,
        ILiteEventHandler<AnswersDeclaredInvalid>,
        ILiteEventHandler<StaticTextsDeclaredValid>,
        ILiteEventHandler<StaticTextsDeclaredInvalid>,
        ILiteEventHandler<QuestionsEnabled>,
        ILiteEventHandler<SubstitutionTitlesChanged>,
        ICompositeEntity,
        IDisposable
    {
        private readonly ILiteEventRegistry liteEventRegistry;
        private readonly IStatefulInterviewRepository interviewRepository;
        private readonly IMvxMainThreadAsyncDispatcher mainThreadDispatcher;

        protected ValidityViewModel() { }

        public ValidityViewModel(ILiteEventRegistry liteEventRegistry,
            IStatefulInterviewRepository interviewRepository,
            IMvxMainThreadAsyncDispatcher mainThreadDispatcher,
            ErrorMessagesViewModel errorMessagesViewModel)
        {
            this.liteEventRegistry = liteEventRegistry;
            this.interviewRepository = interviewRepository;
            this.mainThreadDispatcher = mainThreadDispatcher;
            this.Error = errorMessagesViewModel;
        }

        private string interviewId;

        public Identity Identity { get; set; }

        public void Init(string interviewId, Identity entityIdentity)
        {
            if (entityIdentity == null) throw new ArgumentNullException(nameof(entityIdentity));
            this.interviewId = interviewId;
            this.Identity = entityIdentity;

            this.liteEventRegistry.Subscribe(this, interviewId);
            this.UpdateValidStateAsync();
        }

        private string exceptionErrorMessageFromViewModel;

        private bool isInvalid;
        public bool IsInvalid
        {
            get { return this.isInvalid; }
            private set { this.RaiseAndSetIfChanged(ref this.isInvalid, value); }
        }

        public ErrorMessagesViewModel Error { get; }

        private async Task UpdateValidStateAsync()
        {
            var interview = this.interviewRepository.Get(this.interviewId);

            bool isInvalidEntity = !interview.IsEntityValid(this.Identity);

            bool wasError = this.exceptionErrorMessageFromViewModel != null;

            await mainThreadDispatcher.ExecuteOnMainThreadAsync(() =>
            {
                if (isInvalidEntity && !wasError)
                {
                    var validationMessages = interview.GetFailedValidationMessages(this.Identity, UIResources.Error);

                    this.Error.Caption = UIResources.Validity_Answered_Invalid_ErrorCaption;
                    this.Error.ChangeValidationErrors(validationMessages);
                }
                else if (wasError)
                {
                    this.Error.Caption = UIResources.Validity_NotAnswered_InterviewException_ErrorCaption;
                    this.Error.ChangeValidationErrors(this.exceptionErrorMessageFromViewModel.ToEnumerable());
                }

                this.IsInvalid = isInvalidEntity || wasError;
            });
        }

        public async void Handle(AnswersDeclaredValid @event)
        {
            if (@event.Questions.Contains(this.Identity))
            {
                await this.UpdateValidStateAsync();
            }
        }

        public async void Handle(AnswersDeclaredInvalid @event)
        {
            if (@event.FailedValidationConditions.Keys.Contains(this.Identity))
            {
                await this.UpdateValidStateAsync();
            }
        }

        public async void Handle(StaticTextsDeclaredValid @event)
        {
            if (@event.StaticTexts.Contains(this.Identity))
            {
                await this.UpdateValidStateAsync();
            }
        }

        public async void Handle(StaticTextsDeclaredInvalid @event)
        {
            if (@event.GetFailedValidationConditionsDictionary().Keys.Contains(this.Identity))
            {
                await this.UpdateValidStateAsync();
            }
        }

        public async void Handle(QuestionsEnabled @event)
        {
            if (@event.Questions.Contains(this.Identity))
            {
                await this.UpdateValidStateAsync();
            }
        }


        public async void Handle(SubstitutionTitlesChanged @event)
        {
            if (@event.Questions.Contains(this.Identity) || @event.StaticTexts.Contains(this.Identity))
            {
                await this.UpdateValidStateAsync();
            }
        }
        public virtual async void ProcessException(Exception exception)
        {
            if (exception is InterviewException)
            {
                this.exceptionErrorMessageFromViewModel = exception.Message;

                await this.UpdateValidStateAsync();
            }
        }

        public virtual async void ExecutedWithoutExceptions()
        {
            this.exceptionErrorMessageFromViewModel = null;

            await this.UpdateValidStateAsync();
        }

        public virtual async void MarkAnswerAsNotSavedWithMessage(string errorMessageText)
        {
            this.exceptionErrorMessageFromViewModel = errorMessageText;

            await this.UpdateValidStateAsync();
        }

        public void Dispose()
        {
            this.liteEventRegistry.Unsubscribe(this);
            this.Error.Dispose();
        }
    }
}
