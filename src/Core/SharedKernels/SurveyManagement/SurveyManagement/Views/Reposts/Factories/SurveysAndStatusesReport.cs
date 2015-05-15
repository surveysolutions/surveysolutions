using System;
using System.Collections.Generic;
using System.Linq;
using WB.Core.GenericSubdomains.Utils;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Core.SharedKernels.SurveyManagement.Views.Interview;
using WB.Core.SharedKernels.SurveyManagement.Views.Reposts.InputModels;
using WB.Core.SharedKernels.SurveyManagement.Views.Reposts.Views;

namespace WB.Core.SharedKernels.SurveyManagement.Views.Reposts.Factories
{
    internal class SurveysAndStatusesReport : ISurveysAndStatusesReport
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

                var interviews = (from f in filetredInterviews
                                 group f by new {f.QuestionnaireId, f.QuestionnaireVersion, f.QuestionnaireTitle, f.Status} into g
                                 select new CounterObject
                                 {
                                     QuestionnaireId = g.Key.QuestionnaireId,
                                     QuestionnaireVersion = g.Key.QuestionnaireVersion,
                                     QuestionnaireTitle = g.Key.QuestionnaireTitle,
                                     Status = g.Key.Status,
                                     InterviewsCount = g.Count()
                                 }).ToList();

                var statistics = new List<StatisticsLineGroupedByUserAndTemplate>();
                foreach (var questionnaire in interviews.Select(x => new {x.QuestionnaireId, x.QuestionnaireVersion, x.QuestionnaireTitle}).Distinct())
                {
                    statistics.Add(new StatisticsLineGroupedByUserAndTemplate
                    {
                        QuestionnaireId =               questionnaire.QuestionnaireId,
                        QuestionnaireVersion =          questionnaire.QuestionnaireVersion,
                        QuestionnaireTitle =            questionnaire.QuestionnaireTitle,
                        SupervisorAssignedCount =       Monads.Maybe(() => CountInStatus(interviews, questionnaire, InterviewStatus.SupervisorAssigned).InterviewsCount),
                        InterviewerAssignedCount =      Monads.Maybe(() => CountInStatus(interviews, questionnaire, InterviewStatus.InterviewerAssigned).InterviewsCount),
                        CompletedCount =                Monads.Maybe(() => CountInStatus(interviews, questionnaire, InterviewStatus.Completed).InterviewsCount),
                        ApprovedBySupervisorCount =     Monads.Maybe(() => CountInStatus(interviews, questionnaire, InterviewStatus.ApprovedBySupervisor).InterviewsCount),
                        RejectedBySupervisorCount =     Monads.Maybe(() => CountInStatus(interviews, questionnaire, InterviewStatus.RejectedBySupervisor).InterviewsCount),
                        ApprovedByHeadquartersCount =   Monads.Maybe(() => CountInStatus(interviews, questionnaire, InterviewStatus.ApprovedByHeadquarters).InterviewsCount),
                        RejectedByHeadquartersCount =   Monads.Maybe(() => CountInStatus(interviews, questionnaire, InterviewStatus.RejectedByHeadquarters).InterviewsCount),
                        TotalCount =                    interviews.Where(x => x.QuestionnaireId == questionnaire.QuestionnaireId && x.QuestionnaireVersion == questionnaire.QuestionnaireVersion).Sum(x => x.InterviewsCount)
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

        private static CounterObject CountInStatus(List<CounterObject> interviews, dynamic questionnaire, InterviewStatus status)
        {
            return interviews.FirstOrDefault(x => x.QuestionnaireId == questionnaire.QuestionnaireId && x.QuestionnaireVersion == questionnaire.QuestionnaireVersion && x.Status == status);
        }

        private static IQueryable<InterviewSummary> ApplyFilter(SurveysAndStatusesReportInputModel input, IQueryable<InterviewSummary> _)
        {
            var filetredInterviews = _.Where(x => !x.IsDeleted);

            if (input.ResponsibleId.HasValue)
            {
                filetredInterviews = filetredInterviews.Where(x => x.ResponsibleId == input.ResponsibleId);
            }
            if (input.TeamLeadId.HasValue)
            {
                filetredInterviews = filetredInterviews.Where(x => x.TeamLeadId == input.TeamLeadId);
            }

            return filetredInterviews;
        }

        class CounterObject
        {
            public Guid QuestionnaireId { get; set; }
            public string QuestionnaireTitle { get; set; }
            public long QuestionnaireVersion { get; set; }
            public int InterviewsCount { get; set; }
            public InterviewStatus Status { get; set; }
        }
    }
}
