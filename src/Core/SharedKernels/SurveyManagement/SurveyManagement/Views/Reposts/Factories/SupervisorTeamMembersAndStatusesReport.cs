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
    public class SupervisorTeamMembersAndStatusesReport :
        IViewFactory<SupervisorTeamMembersAndStatusesReportInputModel, SupervisorTeamMembersAndStatusesReportView>
    {
        private readonly IReadSideRepositoryIndexAccessor indexAccessor;

        public SupervisorTeamMembersAndStatusesReport(IReadSideRepositoryIndexAccessor indexAccessor)
        {
            this.indexAccessor = indexAccessor;
        }

        public SupervisorTeamMembersAndStatusesReportView Load(SupervisorTeamMembersAndStatusesReportInputModel input)
        {
            string indexName = typeof (SupervisorReportsTeamMembersAndStatusesGroupByTeamMember).Name;
            IQueryable<StatisticsLineGroupedByUserAndTemplate> items = this.indexAccessor.Query<StatisticsLineGroupedByUserAndTemplate>(indexName)
                                                                           .Where(x => x.TeamLeadId == input.ViewerId);

            items = input.TemplateId.HasValue
                        ? items.Where(x => x.QuestionnaireId == input.TemplateId)
                        : items.Where(x => x.QuestionnaireId == Guid.Empty);

            if (input.TemplateVersion.HasValue)
            {
                items = items.Where(x => x.QuestionnaireVersion == input.TemplateVersion);
            }

            var totalCount = items.Count();

            List<SupervisorTeamMembersAndStatusesReportLine> currentPage =
                items.OrderUsingSortExpression(input.Order)
                     .Skip((input.Page - 1)*input.PageSize)
                     .Take(input.PageSize)
                     .ToList()
                     .Select(doc => new SupervisorTeamMembersAndStatusesReportLine
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
                             ResponsibleId = doc.ResponsibleId,
                             ResponsibleName = doc.ResponsibleName
                         }).ToList();

            return new SupervisorTeamMembersAndStatusesReportView
                {
                    TotalCount = totalCount,
                    Items = currentPage
                };
        }
    }
}