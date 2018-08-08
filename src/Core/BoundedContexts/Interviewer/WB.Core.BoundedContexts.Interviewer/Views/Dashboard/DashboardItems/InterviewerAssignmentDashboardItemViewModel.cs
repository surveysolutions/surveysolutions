using System;
using MvvmCross.Commands;
using WB.Core.BoundedContexts.Interviewer.Services;
using WB.Core.GenericSubdomains.Portable.ServiceLocation;
using WB.Core.SharedKernels.Enumerator.Properties;
using WB.Core.SharedKernels.Enumerator.ViewModels.Dashboard;

namespace WB.Core.BoundedContexts.Interviewer.Views.Dashboard.DashboardItems
{
    public class InterviewerAssignmentDashboardItemViewModel : AssignmentDashboardItemViewModel, IDashboardViewItem
    {
        private IInterviewFromAssignmentCreatorService InterviewFromAssignmentCreator
            => serviceLocator.GetInstance<IInterviewFromAssignmentCreatorService>();

        public int AssignmentId => this.Assignment.Id;

        public InterviewerAssignmentDashboardItemViewModel(IServiceLocator serviceLocator) : base(serviceLocator)
        {
        }

        protected override void BindActions()
        {
            Actions.Clear();
            
            BindLocationAction(Assignment.LocationQuestionId, Assignment?.LocationLatitude, Assignment?.LocationLongitude);
            
            Actions.Add(new ActionDefinition
            {
                Command = new MvxAsyncCommand(
                    () => InterviewFromAssignmentCreator.CreateInterviewAsync(Assignment.Id),
                    () => !Assignment.Quantity.HasValue ||
                          Math.Max(val1: 0, val2: InterviewsLeftByAssignmentCount) > 0),

                Label = InterviewerUIResources.Dashboard_StartNewInterview
            });
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
            }

            base.Dispose(disposing);
        }
    }
}
