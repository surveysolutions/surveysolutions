using System;
using System.Collections.Generic;
using System.Linq;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Core.BoundedContexts.Headquarters.Views.Reposts.InputModels;
using WB.Core.BoundedContexts.Headquarters.Views.Reposts.Views;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Infrastructure.Native.Utils;

namespace WB.Core.BoundedContexts.Headquarters.Views.Reposts.Factories
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
            var inteviewsGroupByTemplateAndStatus = this.interviewSummaryReader.Query(_ =>
            {
                IQueryable<InterviewSummary> filteredInterviews = ApplyFilter(input, _);

                var interviews = (from f in filteredInterviews
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
                        ResponsibleName = input.ResponsibleName ?? string.Empty,
                        TeamLeadName = input.TeamLeadName ?? string.Empty,
                        SupervisorAssignedCount =       CountInStatus(interviews, questionnaire, InterviewStatus.SupervisorAssigned)?.InterviewsCount ?? 0,
                        InterviewerAssignedCount =      CountInStatus(interviews, questionnaire, InterviewStatus.InterviewerAssigned)?.InterviewsCount ?? 0,
                        CompletedCount =                CountInStatus(interviews, questionnaire, InterviewStatus.Completed)?.InterviewsCount ?? 0,
                        ApprovedBySupervisorCount =     CountInStatus(interviews, questionnaire, InterviewStatus.ApprovedBySupervisor)?.InterviewsCount ?? 0,
                        RejectedBySupervisorCount =     CountInStatus(interviews, questionnaire, InterviewStatus.RejectedBySupervisor)?.InterviewsCount ?? 0,
                        ApprovedByHeadquartersCount =   CountInStatus(interviews, questionnaire, InterviewStatus.ApprovedByHeadquarters)?.InterviewsCount ?? 0,
                        RejectedByHeadquartersCount =   CountInStatus(interviews, questionnaire, InterviewStatus.RejectedByHeadquarters)?.InterviewsCount ?? 0,
                        TotalCount =                    interviews.Where(x => x.QuestionnaireId == questionnaire.QuestionnaireId && x.QuestionnaireVersion == questionnaire.QuestionnaireVersion).Sum(x => x.InterviewsCount)
                    });
                }
              
                return statistics;
            });

            List<HeadquarterSurveysAndStatusesReportLine> currentPage = inteviewsGroupByTemplateAndStatus.AsQueryable()
                           .OrderUsingSortExpression(input.Order)
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
                                   
                                   Responsible = GetResponsibleName(doc)
                           }).ToList();

            var totalInterviewCount = currentPage.Sum(x => x.TotalCount);

            currentPage = currentPage.Skip((input.Page - 1) * input.PageSize)
                .Take(input.PageSize).ToList();
            int totalCount = this.interviewSummaryReader.Query(_ =>
            {
                int result = ApplyFilter(input, _)
                            .Select(x => new { x.QuestionnaireId, x.QuestionnaireVersion })
                            .Distinct()
                            .ToList()
                            .Count;

                return result;
            });

            var totalResponsibleCount = this.interviewSummaryReader.Query(_ =>
            {
                IQueryable<InterviewSummary> filetredInterviews = ApplyFilter(input, _);
                return filetredInterviews.Select(x => x.ResponsibleId).Distinct().Count();
            });

            return new SurveysAndStatusesReportView
            {
                TotalResponsibleCount = totalResponsibleCount,
                TotalInterviewCount = totalInterviewCount,
                TotalCount = totalCount,
                Items = currentPage
            };
        }

        private static string GetResponsibleName(StatisticsLineGroupedByUserAndTemplate doc)
        {
            return !string.IsNullOrEmpty(doc.TeamLeadName) ? doc.TeamLeadName : (doc.ResponsibleName ?? string.Empty);
        }

        private static CounterObject CountInStatus(List<CounterObject> interviews, dynamic questionnaire, InterviewStatus status)
        {
            return interviews.FirstOrDefault(x => x.QuestionnaireId == questionnaire.QuestionnaireId && x.QuestionnaireVersion == questionnaire.QuestionnaireVersion && x.Status == status);
        }

        private static IQueryable<InterviewSummary> ApplyFilter(SurveysAndStatusesReportInputModel input, IQueryable<InterviewSummary> _)
        {
            var filteredInterviews = _.Where(x => !x.IsDeleted);

            if (!string.IsNullOrWhiteSpace(input.ResponsibleName))
            {
                filteredInterviews = filteredInterviews.Where(x => x.ResponsibleName.ToLower() == input.ResponsibleName.ToLower());
            }
            if (!string.IsNullOrWhiteSpace(input.TeamLeadName))
            {
                filteredInterviews = filteredInterviews.Where(x => x.TeamLeadName.ToLower() == input.TeamLeadName.ToLower());
            }

            return filteredInterviews;
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
