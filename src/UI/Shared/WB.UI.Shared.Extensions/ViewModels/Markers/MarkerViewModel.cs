using MvvmCross.Commands;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Core.SharedKernels.Enumerator.ViewModels;
using WB.Core.SharedKernels.Enumerator.ViewModels.Markers;
using WB.UI.Shared.Extensions.Extensions;

namespace WB.UI.Shared.Extensions.ViewModels.Markers;

public abstract class MarkerViewModel2: IMarkerViewModel
{
    public abstract string Id { get; }
    public MarkerType Type { get; }
    public string Title { get; set; }
    public string SubTitle { get; set; }
    public int? AssignmentId { get; set; }
    public string InterviewId { get; set; }
    public string InterviewKey { get; set; }
    public InterviewStatus Status { get; set; }
    public string Responsible { get; set; }
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public bool CanAssign { get; set; }
    public bool CanCreateInterview { get; set; }

    private string textDetails;
    public string TextDetails
    {
        get => textDetails ??= ConstructMarkerDetails();
    }

    private string ConstructMarkerDetails()
    {
        var popupTemplate = $"{Title}\r\n{Responsible}";
        if (!string.IsNullOrWhiteSpace(SubTitle))
            popupTemplate += $"\r\n{SubTitle}";

        if (Type == MarkerType.Interview)
        {
            string status = Status.ToLocalizeString();
            if (!string.IsNullOrWhiteSpace(popupTemplate))
                popupTemplate += $"\r\n{status}";
        }

        return popupTemplate;
    }
}

// Supervisor
/*string id = markerViewModel.AssignmentId;
    string title = markerViewModel.Title;
    string subTitle = markerViewModel.SubTitle;
    string responsible = markerViewModel.Responsible;

    var popupTemplate = $"{title}\r\n{responsible}";
    if (!string.IsNullOrWhiteSpace(subTitle))
        popupTemplate += $"\r\n{subTitle}";
    
    if (markerViewModel.Type == MarkerType.Interview)
    {
        string interviewId = markerViewModel.InterviewId;
        string interviewKey = markerViewModel.InterviewKey;
        string status = markerViewModel.Status.ToLocalizeString();
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
        bool canAssign = markerViewModel.CanAssign;

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
    
    
    
        private async Task AssignAssignmentButtonClick(IDictionary<string, object> assignmentInfo, object calloutTag)
    {
        if(calloutTag != null && (Int32.TryParse(calloutTag as string, out int assignmentId)))
        {
            await this.ViewModelNavigationService.NavigateToAsync<SelectResponsibleForAssignmentViewModel, SelectResponsibleForAssignmentArgs>(
                new SelectResponsibleForAssignmentArgs(assignmentId));
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

    */



// Interviewer


    /*
    protected override async Task NavigateToMarkerPopup(IdentifyGraphicsOverlayResult identifyResults, MapPoint projectedLocation)
    {
        string id = identifyResults.Graphics[0].Attributes["id"].ToString();
        string title = identifyResults.Graphics[0].Attributes["title"] as string;
        string subTitle = identifyResults.Graphics[0].Attributes["sub_title"] as string;

        var popupTemplate = $"{title}\r\n{subTitle}";
        
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
            bool canCreate = (bool)assignmentInfo["can_create"];

            CalloutDefinition myCalloutDefinition = new CalloutDefinition("#" + id, popupTemplate);
            if (canCreate)
            {
                myCalloutDefinition.ButtonImage =
                    await new SimpleMarkerSymbol(SimpleMarkerSymbolStyle.Cross, Color.Blue, 25)
                        .CreateSwatchAsync(96);
                myCalloutDefinition.OnButtonClick += tag => CreateFromAssignmentButtonClick(assignmentInfo, tag);
            }

            myCalloutDefinition.Tag = id;
            MapView.ShowCalloutAt(projectedLocation, myCalloutDefinition);
        }
    }

    private void CreateFromAssignmentButtonClick(IDictionary<string, object> assignmentInfo, object calloutTag)
    {
        bool isCreating = assignmentInfo.ContainsKey("creating");
        if (isCreating)
            return;
            
        assignmentInfo["creating"] = true;
        if(calloutTag != null && (Int32.TryParse(calloutTag as string, out int assignmentId)))
        {
            //create interview from assignment
            ViewModelNavigationService.NavigateToCreateAndLoadInterview(assignmentId);
        }
    }
    
    private async void OnInterviewButtonClick(object calloutTag)
    {
        if (calloutTag is string interviewId)
        {
            var interview = interviewViewRepository.GetById(interviewId);
            if (interview != null)
            {
                if (interview.Status == InterviewStatus.Completed)
                {
                    var isReopen = await UserInteractionService.ConfirmAsync(
                        EnumeratorUIResources.Dashboard_Reinitialize_Interview_Message,
                        okButton: UIResources.Yes,
                        cancelButton: UIResources.No);

                    if (!isReopen)
                    {
                        return;
                    }
                }

                await ViewModelNavigationService.NavigateToAsync<LoadingInterviewViewModel, LoadingViewModelArg>(
                    new LoadingViewModelArg
                    {
                        InterviewId = interview.InterviewId,
                        ShouldReopen = true
                    }, true);
            }
        }
    }
    */
