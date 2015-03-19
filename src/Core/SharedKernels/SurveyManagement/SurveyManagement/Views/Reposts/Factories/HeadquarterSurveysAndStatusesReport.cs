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
    public class HeadquarterSurveysAndStatusesReport : IHeadquarterSurveysAndStatusesReport
    {
        private readonly IQueryableReadSideRepositoryReader<InterviewSummary> interviewSummaryReader;

        public HeadquarterSurveysAndStatusesReport(IQueryableReadSideRepositoryReader<InterviewSummary> interviewSummaryReader)
        {
            this.interviewSummaryReader = interviewSummaryReader;
        }

        public HeadquarterSurveysAndStatusesReportView Load(HeadquarterSurveysAndStatusesReportInputModel input)
        {
            var lines = this.interviewSummaryReader.Query(_ =>
            {
                var filetredInterviews = _.Where(x => !x.IsDeleted);

                var createdCount = CountInStatus(input, filetredInterviews, InterviewStatus.Created);
                var supervisorAssignedStatusCount = CountInStatus(input, filetredInterviews, InterviewStatus.SupervisorAssigned);
                var interviewerAssignedCount = CountInStatus(input, filetredInterviews, InterviewStatus.InterviewerAssigned);
                var sentToCapiCount = CountInStatus(input, filetredInterviews, InterviewStatus.SentToCapi);
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
                        CreatedCount =                  Monads.Maybe(() => createdCount.SingleOrDefault(findQuestionnaire).InterviewsCount),
                        SupervisorAssignedCount =       Monads.Maybe(() => supervisorAssignedStatusCount.SingleOrDefault(findQuestionnaire).InterviewsCount),
                        InterviewerAssignedCount =      Monads.Maybe(() => interviewerAssignedCount.SingleOrDefault(findQuestionnaire).InterviewsCount),
                        SentToCapiCount =               Monads.Maybe(() => sentToCapiCount.SingleOrDefault(findQuestionnaire).InterviewsCount),
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
                var result = _.GroupBy(interview => new { interview.QuestionnaireId, interview.QuestionnaireVersion });
                return result.Count();
            }); 

            var currentPage = lines
                           .Select(doc => new HeadquarterSurveysAndStatusesReportLine
                               {
                                   CreatedCount = doc.CreatedCount,
                                   SupervisorAssignedCount = doc.SupervisorAssignedCount,
                                   InterviewerAssignedCount = doc.InterviewerAssignedCount,
                                   SentToCapiCount = doc.SentToCapiCount,
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

            return new HeadquarterSurveysAndStatusesReportView
                {
                    TotalCount = overallCount,
                    Items = currentPage
                };
        }

        private static IEnumerable<CounterObject> CountInStatus(HeadquarterSurveysAndStatusesReportInputModel input, 
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
