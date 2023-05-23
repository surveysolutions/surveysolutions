using System;
using MvvmCross.Commands;
using WB.Core.BoundedContexts.Interviewer.Views.CreateInterview;
using WB.Core.GenericSubdomains.Portable.ServiceLocation;
using WB.Core.SharedKernels.Enumerator.Properties;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.ViewModels.Dashboard;

namespace WB.Core.BoundedContexts.Interviewer.Views.Dashboard.DashboardItems
{
    public class InterviewerAssignmentDashboardItemViewModel : AssignmentDashboardItemViewModel, IDashboardViewItem
    {
        private readonly IViewModelNavigationService viewModelNavigationService;

        public InterviewerAssignmentDashboardItemViewModel(IServiceLocator serviceLocator,
            IViewModelNavigationService viewModelNavigationService) 
            : base(serviceLocator)
        {
            this.viewModelNavigationService = viewModelNavigationService;
        }

        protected override void BindActions()
        {
            Actions.Clear();
            
            BindLocationAction(Assignment.LocationQuestionId, Assignment.LocationLatitude, Assignment.LocationLongitude);
            
            Actions.Add(new ActionDefinition
            {
                Command = new MvxAsyncCommand(
                    async () =>
                    {
                        await viewModelNavigationService
                            .NavigateToAsync<CreateAndLoadInterviewViewModel, CreateInterviewViewModelArg>(
                                new CreateInterviewViewModelArg()
                                {
                                    AssignmentId = Assignment.Id,
                                    InterviewId = Guid.NewGuid()
                                }, true);
                    },
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
        }
    }
}
