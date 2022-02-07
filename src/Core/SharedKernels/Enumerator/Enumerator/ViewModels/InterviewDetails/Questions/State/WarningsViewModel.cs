using System;
using System.Linq;
using System.Threading.Tasks;
using MvvmCross;
using MvvmCross.Base;
using MvvmCross.ViewModels;
using WB.Core.GenericSubdomains.Portable.Tasks;
using WB.Core.Infrastructure.EventBus.Lite;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.Enumerator.Properties;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;

namespace WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions.State
{
    public class WarningsViewModel : MvxNotifyPropertyChanged,
        IAsyncViewModelEventHandler<AnswersDeclaredPlausible>,
        IAsyncViewModelEventHandler<AnswersDeclaredImplausible>,
        IAsyncViewModelEventHandler<StaticTextsDeclaredPlausible>,
        IAsyncViewModelEventHandler<StaticTextsDeclaredImplausible>,
        IAsyncViewModelEventHandler<QuestionsEnabled>,
        IAsyncViewModelEventHandler<SubstitutionTitlesChanged>,
        ICompositeEntity,
        IDisposable
    {
        private readonly IViewModelEventRegistry liteEventRegistry;
        private readonly IStatefulInterviewRepository interviewRepository;
        private readonly IMvxMainThreadAsyncDispatcher mvxMainThreadDispatcher;

        protected WarningsViewModel() { }

        public WarningsViewModel(IViewModelEventRegistry liteEventRegistry,
            IStatefulInterviewRepository interviewRepository,
            ErrorMessagesViewModel errorMessagesViewModel)
        {
            this.liteEventRegistry = liteEventRegistry;
            this.interviewRepository = interviewRepository;
            this.mvxMainThreadDispatcher = Mvx.IoCProvider.Resolve<IMvxMainThreadAsyncDispatcher>();
            this.Warning = errorMessagesViewModel;
        }

        private string interviewId;

        public Identity Identity { get; set; }

        public void Init(string interviewId, Identity entityIdentity, NavigationState navigationState)
        {
            if (entityIdentity == null) throw new ArgumentNullException(nameof(entityIdentity));
            this.interviewId = interviewId;
            this.Identity = entityIdentity;
            this.navigationState = navigationState;
            this.UpdateValidStateAsync().WaitAndUnwrapException();

            this.liteEventRegistry.Subscribe(this, interviewId);
        }
        
        private bool isImplausible;
        private NavigationState navigationState;

        public bool IsImplausible
        {
            get => this.isImplausible;
            private set => this.RaiseAndSetIfChanged(ref this.isImplausible, value);
        }

        public ErrorMessagesViewModel Warning { get; }

        private async Task UpdateValidStateAsync()
        {
            var interview = this.interviewRepository.Get(this.interviewId);

            bool isInvalidEntity = !interview.IsEntityPlausible(this.Identity);

            await this.mvxMainThreadDispatcher.ExecuteOnMainThreadAsync(() =>
            {
                if (isInvalidEntity)
                {
                    var validationMessages = interview.GetFailedWarningMessages(this.Identity, UIResources.Warning);
                    
                    this.Warning.ChangeValidationErrors(validationMessages, this.interviewId, this.Identity, this.navigationState);
                }

                this.IsImplausible = isInvalidEntity;
            });
        }

        public async Task HandleAsync(AnswersDeclaredPlausible @event)
        {
            if (@event.Questions.Contains(this.Identity))
            {
                await this.UpdateValidStateAsync();
            }
        }

        public async Task HandleAsync(AnswersDeclaredImplausible @event)
        {
            if (@event.GetFailedValidationConditionsDictionary().Keys.Contains(this.Identity))
            {
                await this.UpdateValidStateAsync();
            }
        }

        public async Task HandleAsync(StaticTextsDeclaredPlausible @event)
        {
            if (@event.StaticTexts.Contains(this.Identity))
            {
                await this.UpdateValidStateAsync();
            }
        }

        public async Task HandleAsync(StaticTextsDeclaredImplausible @event)
        {
            if (@event.GetFailedValidationConditionsDictionary().Keys.Contains(this.Identity))
            {
                await this.UpdateValidStateAsync();
            }
        }

        public async Task HandleAsync(QuestionsEnabled @event)
        {
            if (@event.Questions.Contains(this.Identity))
            {
                await this.UpdateValidStateAsync();
            }
        }

        public async Task HandleAsync(SubstitutionTitlesChanged @event)
        {
            if (@event.Questions.Contains(this.Identity) || @event.StaticTexts.Contains(this.Identity))
            {
                await this.UpdateValidStateAsync();
            }
        }

        public void Dispose()
        {
            this.liteEventRegistry.Unsubscribe(this);
            this.Warning.Dispose();
        }
    }
}
