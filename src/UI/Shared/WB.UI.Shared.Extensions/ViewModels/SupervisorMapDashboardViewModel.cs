using System.Drawing;
using Esri.ArcGISRuntime.Symbology;
using MvvmCross;
using MvvmCross.Base;
using MvvmCross.Plugin.Messenger;
using MvvmCross.ViewModels;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure.Storage;
using WB.Core.SharedKernels.Enumerator.Services.MapService;
using WB.Core.SharedKernels.Enumerator.ViewModels;
using WB.Core.SharedKernels.Enumerator.ViewModels.Markers;
using WB.Core.SharedKernels.Enumerator.Views;
using WB.UI.Shared.Extensions.Entities;
using WB.UI.Shared.Extensions.Services;


namespace WB.UI.Shared.Extensions.ViewModels;

public class SupervisorMapDashboardViewModel : MapDashboardViewModel
{
    private readonly IPlainStorage<InterviewerDocument> usersRepository;

    protected override InterviewStatus[] InterviewStatuses { get; } =
    {
        InterviewStatus.Created,
        InterviewStatus.InterviewerAssigned,
        InterviewStatus.Restarted,
        InterviewStatus.RejectedBySupervisor,
        InterviewStatus.Completed,
        InterviewStatus.SupervisorAssigned,
        InterviewStatus.RejectedByHeadquarters,
    };

    public SupervisorMapDashboardViewModel(IPrincipal principal, 
        IViewModelNavigationService viewModelNavigationService, 
        IUserInteractionService userInteractionService, 
        IMapService mapService, 
        IAssignmentDocumentsStorage assignmentsRepository, 
        IPlainStorage<InterviewView> interviewViewRepository, 
        IEnumeratorSettings enumeratorSettings, 
        ILogger logger, 
        IMapUtilityService mapUtilityService, 
        IMvxMainThreadAsyncDispatcher mainThreadAsyncDispatcher, 
        IPlainStorage<InterviewerDocument> usersRepository,
        IDashboardViewModelFactory dashboardViewModelFactory,
        IMvxMessenger messenger
        ) 
        : base(principal, viewModelNavigationService, userInteractionService, mapService, assignmentsRepository, interviewViewRepository, enumeratorSettings, logger, mapUtilityService, mainThreadAsyncDispatcher, dashboardViewModelFactory)
    {
        this.usersRepository = usersRepository;
        this.messenger = messenger;
    }

    public override bool SupportDifferentResponsible => true;
    
    protected override void CollectResponsibles()
    {
        List<ResponsibleItem> result = usersRepository.LoadAll()
            .Where(x => !x.IsLockedByHeadquarters && !x.IsLockedBySupervisor)
            .Select(user => new ResponsibleItem(user.InterviewerId, user.UserName))
            .OrderBy(x => x.Title)
            .ToList();

        var responsibleItems = new List<ResponsibleItem>
        {
            AllResponsibleDefault,
            new ResponsibleItem(Principal.CurrentUserIdentity.UserId, Principal.CurrentUserIdentity.Name),
        };
        responsibleItems.AddRange(result);

        Responsibles = new MvxObservableCollection<ResponsibleItem>(responsibleItems);

        if (SelectedResponsible != AllResponsibleDefault)
            SelectedResponsible = AllResponsibleDefault;
    }
    
    private MvxSubscriptionToken messengerSubscription;
    private readonly IMvxMessenger messenger;
    
    private async Task RefreshCounters()
    {
        ReloadEntities();
        await RefreshMarkers();
    }
    
    public override void ViewAppeared()
    {
        base.ViewAppeared();
        messengerSubscription = messenger.Subscribe<DashboardChangedMsg>(async msg => await RefreshCounters(), MvxReference.Strong);
    }

    public override void ViewDisappeared()
    {
        base.ViewDisappeared();
        messengerSubscription?.Dispose();
    }
}
