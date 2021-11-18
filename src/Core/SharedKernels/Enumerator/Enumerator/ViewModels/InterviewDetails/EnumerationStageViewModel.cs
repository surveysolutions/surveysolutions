using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MvvmCross.Base;
using MvvmCross.ViewModels;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;
using WB.Core.SharedKernels.Enumerator.Utils;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Groups;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions.State;


namespace WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails
{
    public class EnumerationStageViewModel : MvxViewModel,
        IAsyncViewModelEventHandler<GroupsDisabled>,
        IDisposable
    {
        private List<IInterviewEntityViewModel> createdEntities = new List<IInterviewEntityViewModel>();

        private CompositeCollection<ICompositeEntity> items;
        public CompositeCollection<ICompositeEntity> Items
        {
            get => this.items;
            set => base.SetProperty(ref items, value);
        }

        private readonly IInterviewViewModelFactory interviewViewModelFactory;
        private readonly IStatefulInterviewRepository interviewRepository;
        private readonly ICompositeCollectionInflationService compositeCollectionInflationService;
        private readonly IViewModelEventRegistry liteEventRegistry;
        private readonly ICommandService commandService;

        readonly IUserInterfaceStateService userInterfaceStateService;

        private NavigationState navigationState;

        string interviewId;
        Identity groupId;

        public DynamicTextViewModel Name { get; }

        public EnumerationStageViewModel(
            IInterviewViewModelFactory interviewViewModelFactory,
            IStatefulInterviewRepository interviewRepository,
            IUserInterfaceStateService userInterfaceStateService,
            DynamicTextViewModel dynamicTextViewModel, 
            ICompositeCollectionInflationService compositeCollectionInflationService,
            IViewModelEventRegistry liteEventRegistry,
            ICommandService commandService)
        {
            this.interviewViewModelFactory = interviewViewModelFactory;
            this.interviewRepository = interviewRepository;
            this.userInterfaceStateService = userInterfaceStateService;
            this.liteEventRegistry = liteEventRegistry;
            this.commandService = commandService;

            this.Name = dynamicTextViewModel;
            this.compositeCollectionInflationService = compositeCollectionInflationService;
            this.Items = new CompositeCollection<ICompositeEntity>();
        }

        public void Configure(string interviewId, NavigationState navigationState, Identity groupId, Identity anchoredElementIdentity)
        {
            if (this.navigationState != null) throw new InvalidOperationException("ViewModel already initialized");

            this.interviewId = interviewId;
            this.groupId = groupId;
            this.navigationState = navigationState ?? throw new ArgumentNullException(nameof(navigationState));
            
            this.InitRegularGroupScreen(groupId, anchoredElementIdentity);

            liteEventRegistry.Subscribe(this, interviewId);
        }

        private void InitRegularGroupScreen(Identity groupIdentity, Identity anchoredElementIdentity)
        {
            this.Name.Init(this.interviewId, groupIdentity);

            this.LoadFromModel(groupIdentity);
            this.SetScrollTo(anchoredElementIdentity);
        }

        private void SetScrollTo(Identity scrollTo)
        {
            if (scrollTo != null)
            {
                ICompositeEntity childItem =
                    (this.Items.OfType<GroupViewModel>().FirstOrDefault(x => x.Identity.Equals(scrollTo)) 
                            ?? (ICompositeEntity) this.Items.OfType<QuestionHeaderViewModel>().FirstOrDefault(x => x.Identity.Equals(scrollTo))) 
                            ?? this.Items.OfType<StaticTextViewModel>().FirstOrDefault(x => x.Identity.Equals(scrollTo));

                this.ScrollToIndex = childItem != null ? this.Items.ToList().IndexOf(childItem) : 0;
            }
        }

        public int? ScrollToIndex { get; set; }

        private void LoadFromModel(Identity groupIdentity)
        {
            try
            {
                this.userInterfaceStateService.NotifyRefreshStarted();

                var previousGroupNavigationViewModel = this.interviewViewModelFactory.GetNew<GroupNavigationViewModel>();
                previousGroupNavigationViewModel.Init(this.interviewId, groupIdentity, this.navigationState);

                List<IInterviewEntityViewModel> entities = this.interviewViewModelFactory.GetEntities(
                    interviewId: this.navigationState.InterviewId,
                    groupIdentity: groupIdentity,
                    navigationState: this.navigationState);

                entities.Add(previousGroupNavigationViewModel);

                var newEntities = this.compositeCollectionInflationService.GetInflatedCompositeCollection(interviewId, entities);

                InvokeOnMainThread(() =>
                {
                    this.Items.ToArray().ForEach(ie => ie.DisposeIfDisposable());
                    this.Items = newEntities;

                    this.createdEntities.ToArray().ForEach(ie => ie.DisposeIfDisposable());
                    this.createdEntities = entities;
                });
            }
            finally
            {
                this.userInterfaceStateService.NotifyRefreshFinished();
            }
        }
        
        public void Dispose()
        {
            this.liteEventRegistry.Unsubscribe(this);
            this.Items.ToArray().ForEach(ie => ie.DisposeIfDisposable());
            this.createdEntities.ToArray().ForEach(ie => ie.DisposeIfDisposable());
            this.Name.Dispose();
        }

        public async Task HandleAsync(GroupsDisabled @event)
        {
            if (@event.Groups.Any(id => id == groupId))
            {
                var interview = this.interviewRepository.Get(this.interviewId);
                var firstSection = interview.GetEnabledSections().First();

                await this.commandService.WaitPendingCommandsAsync();

                await this.navigationState.NavigateTo(NavigationIdentity.CreateForGroup(firstSection.Identity));
            }
        }
    }
}
