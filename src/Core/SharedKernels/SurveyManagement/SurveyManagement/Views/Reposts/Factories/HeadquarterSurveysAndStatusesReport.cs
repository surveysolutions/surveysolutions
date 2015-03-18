using System;
using System.Linq;
using WB.Core.GenericSubdomains.Utils;
using WB.Core.Infrastructure.ReadSide;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.SurveyManagement.Implementation.ReadSide.Indexes;
using WB.Core.SharedKernels.SurveyManagement.Views.Interview;
using WB.Core.SharedKernels.SurveyManagement.Views.Reposts.InputModels;
using WB.Core.SharedKernels.SurveyManagement.Views.Reposts.Views;

namespace WB.Core.SharedKernels.SurveyManagement.Views.Reposts.Factories
{
    public class HeadquarterSurveysAndStatusesReport : IHeadquarterSurveysAndStatusesReport
    {
        private readonly IReadSideRepositoryIndexAccessor indexAccessor;

        public HeadquarterSurveysAndStatusesReport(IReadSideRepositoryIndexAccessor indexAccessor)
        {
            this.indexAccessor = indexAccessor;
        }

        public HeadquarterSurveysAndStatusesReportView Load(HeadquarterSurveysAndStatusesReportInputModel input)
        {
            var indexName = typeof (HeadquarterReportsSurveysAndStatusesGroupByTeam).Name;
            var lines = this.indexAccessor.Query<StatisticsLineGroupedByUserAndTemplate>(indexName);

            lines = input.UserId.HasValue 
                ? lines.Where(x => x.ResponsibleId == input.UserId) 
                : lines.Where(x => x.ResponsibleId == Guid.Empty);

            var totalCount = lines.Count();

            var currentPage = lines.OrderUsingSortExpression(input.Order)
                           .Skip((input.Page - 1)*input.PageSize)
                           .Take(input.PageSize)
                           .ToList()
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

                                   RestoredCount = doc.RestoredCount,
                                   TotalCount = doc.TotalCount,

                                   QuestionnaireId = doc.QuestionnaireId,
                                   QuestionnaireVersion = doc.QuestionnaireVersion,
                                   QuestionnaireTitle = doc.QuestionnaireTitle,

                                   ResponsibleId = doc.ResponsibleId
                               }).ToList();

            return new HeadquarterSurveysAndStatusesReportView
                {
                    TotalCount = totalCount,
                    Items = currentPage
                };
        }
    }
}
