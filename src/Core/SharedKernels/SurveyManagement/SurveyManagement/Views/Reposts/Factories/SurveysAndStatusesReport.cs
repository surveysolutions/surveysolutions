using System;
using System.Collections.Generic;
using System.Linq;
using Raven.Client.Linq;
using WB.Core.GenericSubdomains.Utils;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Core.SharedKernels.SurveyManagement.Views.Interview;
using WB.Core.SharedKernels.SurveyManagement.Views.Reposts.InputModels;
using WB.Core.SharedKernels.SurveyManagement.Views.Reposts.Views;

namespace WB.Core.SharedKernels.SurveyManagement.Views.Reposts.Factories
{
    public class SurveysAndStatusesReport : ISurveysAndStatusesReport
    {
        private readonly IQueryableReadSideRepositoryReader<InterviewSummary> interviewSummaryReader;

        public SurveysAndStatusesReport(IQueryableReadSideRepositoryReader<InterviewSummary> interviewSummaryReader)
        {
            this.interviewSummaryReader = interviewSummaryReader;
        }

        public SurveysAndStatusesReportView Load(SurveysAndStatusesReportInputModel input)
        {
            var lines = this.interviewSummaryReader.Query(_ =>
            {
                var filetredInterviews = ApplyFilter(input, _);

                var supervisorAssignedStatusCount = CountInStatus(input, filetredInterviews, InterviewStatus.SupervisorAssigned);
                var interviewerAssignedCount = CountInStatus(input, filetredInterviews, InterviewStatus.InterviewerAssigned);
                var completedCount = CountInStatus(input, filetredInterviews, InterviewStatus.Completed);
                var approvedBySupervisorCount = CountInStatus(input, filetredInterviews, InterviewStatus.ApprovedBySupervisor);
                var rejectedBySupervisorCount = CountInStatus(input, filetredInterviews, InterviewStatus.RejectedBySupervisor);
                var approvedByHeadquartersCount = CountInStatus(input, filetredInterviews, InterviewStatus.ApprovedByHeadquarters);
                var rejectedByHeadquartersCount = CountInStatus(input, filetredInterviews, InterviewStatus.RejectedByHeadquarters);
                var totalCount = CountInStatus(input, filetredInterviews, null);

                var statistics = new List<StatisticsLineGroupedByUserAndTemplate>();
                foreach (var countResult in supervisorAssignedStatusCount)
                {
                    Func<CounterObject, bool> findQuestionnaire = x => x.QuestionnaireId == countResult.QuestionnaireId && x.QuestionnaireVersion == countResult.QuestionnaireVersion;
                    statistics.Add(new StatisticsLineGroupedByUserAndTemplate
                    {
                        QuestionnaireId = countResult.QuestionnaireId,
                        QuestionnaireVersion = countResult.QuestionnaireVersion,
                        QuestionnaireTitle = countResult.QuestionnaireTitle,
                        SupervisorAssignedCount =       Monads.Maybe(() => supervisorAssignedStatusCount.SingleOrDefault(findQuestionnaire).InterviewsCount),
                        InterviewerAssignedCount =      Monads.Maybe(() => interviewerAssignedCount.SingleOrDefault(findQuestionnaire).InterviewsCount),
                        CompletedCount =                Monads.Maybe(() => completedCount.SingleOrDefault(findQuestionnaire).InterviewsCount),
                        ApprovedBySupervisorCount =     Monads.Maybe(() => approvedBySupervisorCount.SingleOrDefault(findQuestionnaire).InterviewsCount),
                        RejectedBySupervisorCount =     Monads.Maybe(() => rejectedBySupervisorCount.SingleOrDefault(findQuestionnaire).InterviewsCount),
                        ApprovedByHeadquartersCount =   Monads.Maybe(() => approvedByHeadquartersCount.SingleOrDefault(findQuestionnaire).InterviewsCount),
                        RejectedByHeadquartersCount =   Monads.Maybe(() => rejectedByHeadquartersCount.SingleOrDefault(findQuestionnaire).InterviewsCount),
                        TotalCount =                    Monads.Maybe(() => totalCount.SingleOrDefault(findQuestionnaire).InterviewsCount)
                    });
                }

                return statistics;
            });


            var overallCount = this.interviewSummaryReader.Query(_ =>
            {
                var result = ApplyFilter(input, _)
                    .Select(x => x.QuestionnaireId)
                    .Distinct()
                    .Count();
                    
                return result;
            }); 

            var currentPage = lines
                           .Select(doc => new HeadquarterSurveysAndStatusesReportLine
                               {
                                   SupervisorAssignedCount = doc.SupervisorAssignedCount,
                                   InterviewerAssignedCount = doc.InterviewerAssignedCount,
                                   CompletedCount = doc.CompletedCount,
                                   ApprovedBySupervisorCount = doc.ApprovedBySupervisorCount,
                                   RejectedBySupervisorCount = doc.RejectedBySupervisorCount,

                                   ApprovedByHeadquartersCount = doc.ApprovedByHeadquartersCount,
                                   RejectedByHeadquartersCount = doc.RejectedByHeadquartersCount,

                                   TotalCount = doc.TotalCount,

                                   QuestionnaireId = doc.QuestionnaireId,
                                   QuestionnaireVersion = doc.QuestionnaireVersion,
                                   QuestionnaireTitle = doc.QuestionnaireTitle,

                                   ResponsibleId = doc.ResponsibleId
                               }).ToList();

            return new SurveysAndStatusesReportView
                {
                    TotalCount = overallCount,
                    Items = currentPage
                };
        }

        private static IQueryable<InterviewSummary> ApplyFilter(SurveysAndStatusesReportInputModel input, IQueryable<InterviewSummary> _)
        {
            var filetredInterviews = _.Where(x => !x.IsDeleted);

            if (input.UserId.HasValue)
            {
                filetredInterviews = filetredInterviews.Where(x => x.ResponsibleId == input.UserId);
            }
            if (input.ViewerId.HasValue)
            {
                filetredInterviews = filetredInterviews.Where(x => x.TeamLeadId == input.ViewerId);
            }

            return filetredInterviews;
        }

        private static IEnumerable<CounterObject> CountInStatus(SurveysAndStatusesReportInputModel input, 
            IQueryable<InterviewSummary> filetredInterviews, 
            InterviewStatus? requestedStatus)
        {
            var query = filetredInterviews;
            if (requestedStatus.HasValue)
            {
                query = query.Where(x => x.Status == requestedStatus);
            }

            var result = query.GroupBy(interview => new { interview.QuestionnaireId, interview.QuestionnaireVersion, interview.QuestionnaireTitle })
                    .Select(g => new CounterObject
                    {
                        QuestionnaireId = g.Key.QuestionnaireId,
                        QuestionnaireVersion = g.Key.QuestionnaireVersion,
                        QuestionnaireTitle = g.Key.QuestionnaireTitle,
                        InterviewsCount = g.Count()
                    })
                    .Skip((input.Page - 1)*input.PageSize)
                    .Take(input.PageSize)
                    .ToList();

            return result;
        }

        class CounterObject
        {
            public Guid QuestionnaireId { get; set; }
            public string QuestionnaireTitle { get; set; }
            public long QuestionnaireVersion { get; set; }
            public int InterviewsCount { get; set; }
        }
    }
}
