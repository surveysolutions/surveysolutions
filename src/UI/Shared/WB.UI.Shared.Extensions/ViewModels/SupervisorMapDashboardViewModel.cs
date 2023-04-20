using System.Drawing;
using Esri.ArcGISRuntime.Data;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Symbology;
using Esri.ArcGISRuntime.UI;
using MvvmCross.Base;
using MvvmCross.ViewModels;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Core.SharedKernels.Enumerator.Properties;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure.Storage;
using WB.Core.SharedKernels.Enumerator.Services.MapService;
using WB.Core.SharedKernels.Enumerator.ViewModels;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewLoading;
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
        IPlainStorage<InterviewerDocument> usersRepository) 
        : base(principal, viewModelNavigationService, userInteractionService, mapService, assignmentsRepository, interviewViewRepository, enumeratorSettings, logger, mapUtilityService, mainThreadAsyncDispatcher)
    {
        this.usersRepository = usersRepository;
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
    
    protected override Symbol GetInterviewMarkerSymbol(InterviewView interview)
    {
        Color markerColor;

        switch (interview.Status)
        {
            case InterviewStatus.Created:
            case InterviewStatus.InterviewerAssigned:
            case InterviewStatus.Restarted:    
            case InterviewStatus.ApprovedBySupervisor:
            case InterviewStatus.RejectedBySupervisor:
                markerColor = Color.FromArgb(0x1f,0x95,0x00);
                break;
            case InterviewStatus.Completed:
                markerColor = Color.FromArgb(0x2a, 0x81, 0xcb);
                break;
            case InterviewStatus.RejectedByHeadquarters:
                markerColor = Color.FromArgb(0xe4,0x51,0x2b);
                break;
            default:
                markerColor = Color.Yellow;
                break;
        }

        return new CompositeSymbol(new[]
        {
            new SimpleMarkerSymbol(SimpleMarkerSymbolStyle.Circle, Color.White, 22), //for contrast
            new SimpleMarkerSymbol(SimpleMarkerSymbolStyle.Circle, markerColor, 16)
        });
    }
    
    protected override KeyValuePair<string, object>[] GetInterviewAttributes(InterviewView interview)
    {
        var baseAttributes = base.GetInterviewAttributes(interview);

        var responsibleName = Responsibles.FirstOrDefault(r => interview.ResponsibleId == r.ResponsibleId)?.Title;

        return baseAttributes.Concat(new[]
        {
            new KeyValuePair<string, object>("responsible", responsibleName),
        }).ToArray();
    }

    protected override KeyValuePair<string, object>[] GetAssignmentAttributes(AssignmentDocument assignment)
    {
        var baseAttributes = base.GetAssignmentAttributes(assignment);

        return baseAttributes.Concat(new[]
        {
            new KeyValuePair<string, object>("responsible", assignment.ResponsibleName),
            new KeyValuePair<string, object>("can_assign", true)
        }).ToArray();
    }
    
    protected override async Task ShowMapPopup(IdentifyGraphicsOverlayResult identifyResults, MapPoint projectedLocation)
    {
        string id = identifyResults.Graphics[0].Attributes["id"].ToString();
        string title = identifyResults.Graphics[0].Attributes["title"] as string;
        string subTitle = identifyResults.Graphics[0].Attributes["sub_title"] as string;
        string responsible = identifyResults.Graphics[0].Attributes["responsible"] as string;

        var popupTemplate = $"{title}\r\n{responsible}";
        if (!string.IsNullOrWhiteSpace(subTitle))
            popupTemplate += $"\r\n{subTitle}";
        
        if (string.IsNullOrEmpty(id))
        {
            string interviewId = identifyResults.Graphics[0].Attributes["interviewId"].ToString();
            string interviewKey = identifyResults.Graphics[0].Attributes["interviewKey"].ToString();
            string status = identifyResults.Graphics[0].Attributes["status"].ToString();
            if (!string.IsNullOrWhiteSpace(popupTemplate))
                popupTemplate += $"\r\n{status}";

            CalloutDefinition myCalloutDefinition =
                new CalloutDefinition(interviewKey, popupTemplate)
                {
                    ButtonImage = await new SimpleMarkerSymbol(SimpleMarkerSymbolStyle.Circle,
                             Color.Blue, 25).CreateSwatchAsync(96)
                };

            myCalloutDefinition.OnButtonClick += OnInterviewButtonClick;
            myCalloutDefinition.Tag = interviewId;
            
            MapView.ShowCalloutAt(projectedLocation, myCalloutDefinition);
        }
        else
        {
            var assignmentInfo = identifyResults.Graphics[0].Attributes;
            bool canAssign = (bool)assignmentInfo["can_assign"];

            CalloutDefinition myCalloutDefinition = new CalloutDefinition("#" + id, popupTemplate);
            if (canAssign)
            {
                myCalloutDefinition.ButtonImage = await new TextSymbol("⋮", Color.Blue, 25,
                    HorizontalAlignment.Center, VerticalAlignment.Middle).CreateSwatchAsync(96);
                myCalloutDefinition.OnButtonClick += async (tag) => await AssignAssignmentButtonClick(assignmentInfo, tag);
            }

            myCalloutDefinition.Tag = id;
            MapView.ShowCalloutAt(projectedLocation, myCalloutDefinition);
        }
    }

    private async Task AssignAssignmentButtonClick(IDictionary<string, object> assignmentInfo, object calloutTag)
    {
        if(calloutTag != null && (Int32.TryParse(calloutTag as string, out int assignmentId)))
        {
            await this.ViewModelNavigationService.NavigateToAsync<SelectResponsibleForAssignmentViewModel, SelectResponsibleForAssignmentArgs>(
                new SelectResponsibleForAssignmentArgs(assignmentId));
            
            ReloadEntities();
            await RefreshMarkers();
        }
    }
    
    private async void OnInterviewButtonClick(object calloutTag)
    {
        if (calloutTag is string interviewId)
        {
            var interview = interviewViewRepository.GetById(interviewId);
            if (interview != null)
            {
                await ViewModelNavigationService.NavigateToAsync<LoadingInterviewViewModel, LoadingViewModelArg>(
                    new LoadingViewModelArg
                    {
                        InterviewId = interview.InterviewId
                    }, true);
            }
        }
    }
}
