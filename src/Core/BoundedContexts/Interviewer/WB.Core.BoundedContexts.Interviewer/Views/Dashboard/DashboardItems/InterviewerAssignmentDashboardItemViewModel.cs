﻿using System;
using MvvmCross.Commands;
using WB.Core.BoundedContexts.Interviewer.Services;
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
            
            BindLocationAction(Assignment.LocationQuestionId, Assignment?.LocationLatitude, Assignment?.LocationLongitude);
            
            Actions.Add(new ActionDefinition
            {
                Command = new MvxAsyncCommand(
                    () => viewModelNavigationService.NavigateToAsync<CreateAndLoadInterviewViewModel, CreateInterviewViewModelArg>(
                        new CreateInterviewViewModelArg()
                        {
                            AssignmentId = Assignment.Id,
                            InterviewId = Guid.NewGuid()
                        }),
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
