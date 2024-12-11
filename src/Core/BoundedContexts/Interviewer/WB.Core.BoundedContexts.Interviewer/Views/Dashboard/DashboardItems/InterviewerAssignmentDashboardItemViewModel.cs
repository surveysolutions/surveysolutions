using System;
using System.Threading.Tasks;
using MvvmCross.Commands;
using WB.Core.BoundedContexts.Interviewer.Views.CreateInterview;
using WB.Core.GenericSubdomains.Portable.ServiceLocation;
using WB.Core.SharedKernels.Enumerator.Properties;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;
using WB.Core.SharedKernels.Enumerator.ViewModels.Dashboard;

namespace WB.Core.BoundedContexts.Interviewer.Views.Dashboard.DashboardItems
{
    public class InterviewerAssignmentDashboardItemViewModel : AssignmentDashboardItemViewModel, IDashboardViewItem
    {
        private readonly IViewModelNavigationService viewModelNavigationService;
        private readonly IMapInteractionService mapInteractionService;
        private readonly IUserInteractionService userInteractionService;

        public InterviewerAssignmentDashboardItemViewModel(IServiceLocator serviceLocator,
            IViewModelNavigationService viewModelNavigationService,
            IMapInteractionService mapInteractionService,
            IUserInteractionService userInteractionService) 
            : base(serviceLocator)
        {
            this.viewModelNavigationService = viewModelNavigationService;
            this.mapInteractionService = mapInteractionService;
            this.userInteractionService = userInteractionService;
        }

        protected override void BindActions()
        {
            Actions.Clear();
            
            BindLocationAction(Assignment.LocationQuestionId, Assignment.LocationLatitude, Assignment.LocationLongitude);
            
            Actions.Add(new ActionDefinition
            {
                Command = new MvxAsyncCommand(
                    async () => { await CreateInterviewAsync(); },
                    () => !Assignment.Quantity.HasValue ||
                          Math.Max(val1: 0, val2: InterviewsLeftByAssignmentCount) > 0),

                Label = EnumeratorUIResources.Dashboard_StartNewInterview
            });
            
            Actions.Add(new ActionDefinition
            {
                ActionType = ActionType.Context,
                Command = new MvxAsyncCommand(this.SetCalendarEventAsync),
                Label = Assignment.CalendarEvent.HasValue 
                    ? EnumeratorUIResources.Dashboard_EditCalendarEvent
                    : EnumeratorUIResources.Dashboard_AddCalendarEvent
            });

            Actions.Add(new ActionDefinition
            {
                ActionType = ActionType.Context,
                Command = new MvxCommand(this.RemoveCalendarEvent, () => Assignment.CalendarEvent.HasValue),
                Label = EnumeratorUIResources.Dashboard_RemoveCalendarEvent
            });

            BindTargetAreaAction(Assignment.Id, Assignment.TargetArea);
        }

        private async Task CreateInterviewAsync()
        {
            var confirmResult = await userInteractionService.ConfirmAsync(
                EnumeratorUIResources.Dashboard_CreateInterview_TargetArea_Warning);
            if (!confirmResult)
                return;
            
            await viewModelNavigationService
                .NavigateToAsync<CreateAndLoadInterviewViewModel, CreateInterviewViewModelArg>(
                    new CreateInterviewViewModelArg()
                    {
                        AssignmentId = Assignment.Id,
                        InterviewId = Guid.NewGuid()
                    }, true);
        }

        protected void BindTargetAreaAction(int assignmentId, string targetArea)
        {
            if (string.IsNullOrWhiteSpace(targetArea)) return;

            Actions.Add(new ActionDefinition
            {
                Command = new MvxAsyncCommand(
                    () => mapInteractionService.OpenAssignmentMapAsync(assignmentId)),

                ActionType = ActionType.TargetArea,

                Label = EnumeratorUIResources.Dashboard_ShowAssignmentMap
            });
        }
    }
}
