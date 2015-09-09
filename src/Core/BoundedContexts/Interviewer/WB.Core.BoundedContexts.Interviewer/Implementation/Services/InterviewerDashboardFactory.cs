using System;
using System.Collections.Generic;
using Cirrious.CrossCore;
using WB.Core.BoundedContexts.Interviewer.Services;
using WB.Core.BoundedContexts.Interviewer.Views.Dashboard;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Core.SharedKernels.Enumerator.Aggregates;
using WB.Core.SharedKernels.Enumerator.Repositories;


namespace WB.Core.BoundedContexts.Interviewer.Implementation.Services
{
    public class InterviewerDashboardFactory : IInterviewerDashboardFactory
    {
        private readonly IStatefulInterviewRepository aggregateRootRepository;

        public InterviewerDashboardFactory(IStatefulInterviewRepository aggregateRootRepository)
        {
            this.aggregateRootRepository = aggregateRootRepository;
        }

        public DashboardInformation GetDashboardItems(Guid interviewerId)
        {
            DashboardInformation dashboardInformation = new DashboardInformation();
            dashboardInformation.DashboardItems = new List<InterviewDashboardItemViewModel>();

            var interviewAggregateRoots = aggregateRootRepository.GetAll();

            foreach (var interview in interviewAggregateRoots)
            {
                var category = this.GetDashboardCategoryForInterview(interview);
                IncreaseDashboardStatisticCounter(dashboardInformation, category);

                var interviewDashboardItem = Mvx.Resolve<InterviewDashboardItemViewModel>();
                interviewDashboardItem.Init(interview);

                dashboardInformation.DashboardItems.Add(interviewDashboardItem);
            }

            return dashboardInformation;
        }

        private DashboardInterviewCategories GetDashboardCategoryForInterview(IStatefulInterview interview)
        {
            switch (interview.Status)
            {
                case InterviewStatus.RejectedBySupervisor:
                    return DashboardInterviewCategories.Rejected;
                case InterviewStatus.Completed:
                    return DashboardInterviewCategories.Complited;
                case InterviewStatus.Restarted:
                    return DashboardInterviewCategories.InProgress;
                case InterviewStatus.InterviewerAssigned:
                {
                    if (interview.Answers.Count > 0)
                        return DashboardInterviewCategories.InProgress;
                    else
                        return DashboardInterviewCategories.New;
                }

                default:
                    throw new ArgumentException("Can't identify status for interview: {0}".FormatString(interview.Id));
            }
        }

        private void IncreaseDashboardStatisticCounter(DashboardInformation dashboardInformation, DashboardInterviewCategories category)
        {
            switch (category)
            {
                case DashboardInterviewCategories.Rejected:
                    dashboardInformation.RejectedInterviewsCount++;
                    break;
                case DashboardInterviewCategories.Complited:
                    dashboardInformation.CompletedInterviewsCount++;
                    break;
                case DashboardInterviewCategories.New:
                    dashboardInformation.NewInterviewsCount++;
                    break;
                case DashboardInterviewCategories.InProgress:
                    dashboardInformation.StartedInterviewsCount++;
                    break;
            }
        }
    }
}