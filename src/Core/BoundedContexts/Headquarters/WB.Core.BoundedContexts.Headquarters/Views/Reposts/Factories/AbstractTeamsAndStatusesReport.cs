using System;
using System.Linq;
using System.Linq.Expressions;
using NHibernate;
using NHibernate.Criterion;
using NHibernate.Transform;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Core.BoundedContexts.Headquarters.Views.Reposts.InputModels;
using WB.Core.BoundedContexts.Headquarters.Views.Reposts.Views;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Infrastructure.Native.Storage;
using WB.Infrastructure.Native.Utils;

namespace WB.Core.BoundedContexts.Headquarters.Views.Reposts.Factories
{
    internal abstract class AbstractTeamsAndStatusesReport
    {
        private readonly INativeReadSideStorage<InterviewSummary> interviewsReader;

        protected AbstractTeamsAndStatusesReport(INativeReadSideStorage<InterviewSummary> interviewsReader)
        {
            this.interviewsReader = interviewsReader;
        }

        public TeamsAndStatusesReportView Load(TeamsAndStatusesInputModel input)
        {
            var filter = CreateFilterExpression(input);

            var rowCount = this.interviewsReader.CountDistinctWithRecursiveIndex(_ => _.Where(filter)
                .Select(this.ResponsibleIdSelector));

            var statistics =
                this.interviewsReader.QueryOver(
                    _ => _.Where(filter).Select(
                        this.AddCountsByStuses(Projections.ProjectionList())
                            .Add(Projections.Group(this.ResponsibleIdSelector), "ResponsibleId")
                            .Add(Projections.Min(Projections.Property(this.ResponsibleNameSelector)), "Responsible")));

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


            var totalStatistics =
                this.interviewsReader.QueryOver(
                    _ => _.Where(filter)
                    .Select(this.AddCountsByStuses(Projections.ProjectionList())))
                    .TransformUsing(Transformers.AliasToBean<TeamsAndStatusesReportLine>())
                    .SingleOrDefault<TeamsAndStatusesReportLine>();

            return new TeamsAndStatusesReportView
            {
                TotalCount = rowCount,
                Items = currentPage,
                TotalRow = totalStatistics
            };
        }

        private ProjectionList AddCountsByStuses(ProjectionList projectionList)
        {
            return projectionList
                .Add(this.CountByStatus(InterviewStatus.SupervisorAssigned), "SupervisorAssignedCount")
                .Add(this.CountByStatus(InterviewStatus.InterviewerAssigned), "InterviewerAssignedCount")
                .Add(this.CountByStatus(InterviewStatus.Restarted), "RestartedCount")
                .Add(this.CountByStatus(InterviewStatus.Completed), "CompletedCount")
                .Add(this.CountByStatus(InterviewStatus.ApprovedBySupervisor), "ApprovedBySupervisorCount")
                .Add(this.CountByStatus(InterviewStatus.RejectedBySupervisor), "RejectedBySupervisorCount")
                .Add(this.CountByStatus(InterviewStatus.ApprovedByHeadquarters), "ApprovedByHeadquartersCount")
                .Add(this.CountByStatus(InterviewStatus.RejectedByHeadquarters), "RejectedByHeadquartersCount")
                .Add(Projections.RowCount(), "TotalCount");
        }

        private AggregateProjection CountByStatus(InterviewStatus status)
        {
            return Projections.Sum(Projections.Conditional(
                    Restrictions.Where<InterviewSummary>(i => i.Status == status),
                    Projections.Constant(1),
                    Projections.Constant(0)));
        }

        protected virtual Expression<Func<InterviewSummary, bool>> CreateFilterExpression(TeamsAndStatusesInputModel input)
        {
            Expression<Func<InterviewSummary, bool>> result = (interview) => !interview.IsDeleted;

            if (input.TemplateId.HasValue)
            {
                result = result.AndCondition(x => x.QuestionnaireId == input.TemplateId);
            }

            if (input.TemplateVersion.HasValue)
            {
                result = result.AndCondition(x => x.QuestionnaireVersion == input.TemplateVersion);
            }

            if (input.ViewerId.HasValue)
            {
                result = result.AndCondition(x => x.TeamLeadId == input.ViewerId);
            }
            return result;
        }
        protected abstract Expression<Func<InterviewSummary, object>> ResponsibleIdSelector { get; }
        protected abstract Expression<Func<InterviewSummary, object>> ResponsibleNameSelector { get; }
    }
}