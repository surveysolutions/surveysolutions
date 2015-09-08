using System;
using System.Collections.Generic;
using Cirrious.CrossCore;
using WB.Core.BoundedContexts.Interviewer.Services;
using WB.Core.BoundedContexts.Interviewer.Views.Dashboard;
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

        public IEnumerable<InterviewDashboardItemViewModel> GetDashboardItems(Guid interviewerId)
        {
            var interviewAggregateRoots = aggregateRootRepository.GetAll();

            foreach (var interview in interviewAggregateRoots)
            {
                var interviewDashboardItem = Mvx.Resolve<InterviewDashboardItemViewModel>();
                interviewDashboardItem.Init(interview);

                yield return interviewDashboardItem;
            }
        }
    }
}