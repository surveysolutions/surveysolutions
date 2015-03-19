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
    public class SupervisorSurveysAndStatusesReport :
        IViewFactory<SupervisorSurveysAndStatusesReportInputModel, SupervisorSurveysAndStatusesReportView>
    {
        private readonly IReadSideRepositoryIndexAccessor indexAccessor;

        public SupervisorSurveysAndStatusesReport(IReadSideRepositoryIndexAccessor indexAccessor)
        {
            this.indexAccessor = indexAccessor;
        }

        public SupervisorSurveysAndStatusesReportView Load(SupervisorSurveysAndStatusesReportInputModel input)
        {
            string indexName = typeof (SupervisorReportsSurveysAndStatusesGroupByTeamMember).Name;
            IQueryable<StatisticsLineGroupedByUserAndTemplate> items = this.indexAccessor.Query<StatisticsLineGroupedByUserAndTemplate>(indexName)
                                                                           .Where(x => x.TeamLeadId == input.ViewerId);

            items = input.UserId.HasValue
                        ? items.Where(x => x.ResponsibleId == input.UserId)
                        : items.Where(x => x.ResponsibleId == Guid.Empty);


            var totalCount = items.Count();

            List<SupervisorSurveysAndStatusesReportLine> currentPage =
                items.OrderUsingSortExpression(input.Order)
                     .Skip((input.Page - 1)*input.PageSize)
                     .Take(input.PageSize)
                     .ToList()
                     .Select(doc => new SupervisorSurveysAndStatusesReportLine
                         {
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

            return new SupervisorSurveysAndStatusesReportView
                {
                    TotalCount = totalCount,
                    Items = currentPage
                };
        }
    }
}