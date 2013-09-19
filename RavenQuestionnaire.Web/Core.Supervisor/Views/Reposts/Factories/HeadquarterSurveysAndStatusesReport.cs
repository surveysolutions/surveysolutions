using System;
using System.Collections.Generic;
using System.Linq;
using Core.Supervisor.Views.Reposts.InputModels;
using Core.Supervisor.Views.Reposts.Views;
using Core.Supervisor.Views.Survey;
using Main.Core.Utility;
using Main.Core.View;
using WB.Core.BoundedContexts.Supervisor.Views.Interview;
using WB.Core.Infrastructure.Raven.Implementation.ReadSide.Indexes;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;

namespace Core.Supervisor.Views.Reposts.Factories
{
    public class HeadquarterSurveysAndStatusesReport : IViewFactory<HeadquarterSurveysAndStatusesReportInputModel, HeadquarterSurveysAndStatusesReportView>
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
