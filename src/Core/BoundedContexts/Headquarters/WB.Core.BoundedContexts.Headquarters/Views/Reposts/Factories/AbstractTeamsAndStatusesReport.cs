﻿using System;
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
            var rowCount = this.interviewsReader.QueryOver(_ => this.ApplyFilter(input, _)
                .Select(
                    Projections.Count(Projections.Distinct(Projections.Property(this.ResponsibleIdSelector)))))
                .SingleOrDefault<int>();

            var statistics =
                this.interviewsReader.QueryOver(
                    _ => this.ApplyFilter(input, _).Select(
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
                    _ => this.ApplyFilter(input, _)
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

        protected virtual IQueryOver<InterviewSummary, InterviewSummary> ApplyFilter(TeamsAndStatusesInputModel input,
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

        protected abstract Expression<Func<InterviewSummary, object>> ResponsibleIdSelector { get; }
        protected abstract Expression<Func<InterviewSummary, object>> ResponsibleNameSelector { get; }
    }
}