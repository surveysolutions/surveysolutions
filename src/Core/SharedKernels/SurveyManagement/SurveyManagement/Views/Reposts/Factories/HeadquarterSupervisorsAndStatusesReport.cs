using System;
using System.Collections.Generic;
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
    public class HeadquarterSupervisorsAndStatusesReport : IViewFactory<HeadquarterSupervisorsAndStatusesReportInputModel, HeadquarterSupervisorsAndStatusesReportView>
    {
        private readonly IReadSideRepositoryIndexAccessor indexAccessor;

        public HeadquarterSupervisorsAndStatusesReport(IReadSideRepositoryIndexAccessor indexAccessor)
        {
            this.indexAccessor = indexAccessor;
        }

        public HeadquarterSupervisorsAndStatusesReportView Load(HeadquarterSupervisorsAndStatusesReportInputModel input)
        {
            string indexName = typeof (HeadquarterReportsTeamsAndStatusesGroupByTeam).Name;
            var items = this.indexAccessor.Query<StatisticsLineGroupedByUserAndTemplate>(indexName);

            items = input.TemplateId.HasValue
                        ? items.Where(x => x.QuestionnaireId == input.TemplateId)
                        : items.Where(x => x.QuestionnaireId == Guid.Empty);

            if (input.TemplateVersion.HasValue)
            {
                items = items.Where(x => x.QuestionnaireVersion == input.TemplateVersion);
            }

            var totalCount = items.Count();

            List<HeadquarterSupervisorsAndStatusesReportLine> currentPage = items.OrderUsingSortExpression(input.Order)
                                                                         .Skip((input.Page - 1)*input.PageSize)
                                                                         .Take(input.PageSize)
                                                                         .ToList()
                                                                         .Select(doc =>
                                                                                 new HeadquarterSupervisorsAndStatusesReportLine
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
                                                                                         ResponsibleId = doc.ResponsibleId,
                                                                                         ResponsibleName = doc.ResponsibleName
                                                                                     }).ToList();

            return new HeadquarterSupervisorsAndStatusesReportView
                {
                    TotalCount = totalCount,
                    Items = currentPage
                };
        }
    }
}