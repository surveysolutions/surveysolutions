using System;
using System.Collections.Generic;
using System.Linq;
using NHibernate;
using NHibernate.Criterion;
using NHibernate.Transform;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Core.SharedKernels.SurveyManagement.Views.Interview;
using WB.Core.SharedKernels.SurveyManagement.Views.Reposts.InputModels;
using WB.Core.SharedKernels.SurveyManagement.Views.Reposts.Views;
using WB.Infrastructure.Native.Storage;

namespace WB.Core.SharedKernels.SurveyManagement.Views.Reposts.Factories
{
    internal class HeadquartersTeamsAndStatusesReport : IHeadquartersTeamsAndStatusesReport
    {
        private readonly INativeReadSideStorage<InterviewSummary> interviewsReader;

        public HeadquartersTeamsAndStatusesReport(INativeReadSideStorage<InterviewSummary> interviewsReader)
        {
            this.interviewsReader = interviewsReader;
        }

        public TeamsAndStatusesReportView Load(TeamsAndStatusesInputModel input)
        {
            var rowCount = this.interviewsReader.QueryOver(_ => ApplyFilter(input, _)
                        .Select(Projections.Count(Projections.Distinct(Projections.Property<InterviewSummary>(x => x.TeamLeadId)))))
                        .SingleOrDefault<int>();

            var statistics =
                this.interviewsReader.QueryOver(
                    _ => ApplyFilter(input, _).Select(
                        Projections.ProjectionList()
                            .Add(Projections.Group<InterviewSummary>(c => c.TeamLeadId), "ResponsibleId")
                            .Add(Projections.Min(Projections.Property<InterviewSummary>(i => i.TeamLeadName)), "Responsible")
                            .Add(AddCountByStatus(InterviewStatus.SupervisorAssigned), "SupervisorAssignedCount")
                            .Add(AddCountByStatus(InterviewStatus.InterviewerAssigned), "InterviewerAssignedCount")
                            .Add(AddCountByStatus(InterviewStatus.Completed), "CompletedCount")
                            .Add(AddCountByStatus(InterviewStatus.ApprovedBySupervisor), "ApprovedBySupervisorCount")
                            .Add(AddCountByStatus(InterviewStatus.RejectedBySupervisor), "RejectedBySupervisorCount")
                            .Add(AddCountByStatus(InterviewStatus.ApprovedByHeadquarters), "ApprovedByHeadquartersCount")
                            .Add(AddCountByStatus(InterviewStatus.RejectedByHeadquarters), "RejectedByHeadquartersCount")
                            .Add(Projections.RowCount(), "TotalCount")
                        ));

            var sorting = QueryableExtensions.ParseSortExpression(input.Order);
            if (sorting != null)
            {
                foreach (var orderRequestItem in sorting)
                {
                    statistics.UnderlyingCriteria.AddOrder(new Order(orderRequestItem.Field,
                        orderRequestItem.Direction == OrderDirection.Asc));
                }
            }

            var currentPage =
                statistics.TransformUsing(Transformers.AliasToBean<TeamsAndStatusesReportLine>())
                    .Skip((input.Page - 1)*input.PageSize)
                    .Take(input.PageSize).List<TeamsAndStatusesReportLine>();

            return new TeamsAndStatusesReportView
            {
                TotalCount = rowCount,
                Items = currentPage
            };
        }

        private AggregateProjection AddCountByStatus(InterviewStatus status)
        {
            return Projections.Sum(
                Projections.Conditional(
                    Restrictions.Where<InterviewSummary>(
                        i => i.Status == status),
                    Projections.Constant(1),
                    Projections.Constant(0)));
        }

        private static IQueryOver<InterviewSummary, InterviewSummary> ApplyFilter(TeamsAndStatusesInputModel input,
            IQueryOver<InterviewSummary, InterviewSummary> interviews)
        {
            var filteredInterviews = interviews.Where(x => !x.IsDeleted);

            if (input.TemplateId.HasValue)
            {
                filteredInterviews = filteredInterviews.Where(x => x.QuestionnaireId == input.TemplateId);
            }

            if (input.TemplateVersion.HasValue)
            {
                filteredInterviews = filteredInterviews.Where(x => x.QuestionnaireVersion == input.TemplateVersion);
            }

            if (input.ViewerId.HasValue)
            {
                filteredInterviews = filteredInterviews.Where(x => x.TeamLeadId == input.ViewerId);
            }

            return filteredInterviews;
        }
    }
}