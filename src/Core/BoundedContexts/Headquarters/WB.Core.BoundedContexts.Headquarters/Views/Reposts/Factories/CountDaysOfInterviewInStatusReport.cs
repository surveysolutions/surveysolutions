using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Main.Core.Entities.SubEntities;
using Npgsql;
using OfficeOpenXml.FormulaParsing.Excel.Functions.DateTime;
using WB.Core.BoundedContexts.Headquarters.Assignments;
using WB.Core.BoundedContexts.Headquarters.Resources;
using WB.Core.BoundedContexts.Headquarters.Views.DataExport;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Core.BoundedContexts.Headquarters.Views.Reports.InputModels;
using WB.Core.BoundedContexts.Headquarters.Views.Reports.Views;
using WB.Core.BoundedContexts.Headquarters.Views.Reposts.Views;
using WB.Core.BoundedContexts.Headquarters.Views.User;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Infrastructure.Native.Fetching;
using WB.Infrastructure.Native.Storage.Postgre;


namespace WB.Core.BoundedContexts.Headquarters.Views.Reports.Factories
{
    public class CountDaysOfInterviewInStatusReport : ICountDaysOfInterviewInStatusReport
    {
        private readonly PostgresPlainStorageSettings plainStorageSettings;
        private readonly IPlainStorageAccessor<Assignment> assignmentsStorage;

        public CountDaysOfInterviewInStatusReport(
            IPlainStorageAccessor<Assignment> assignmentsStorage, 
            PostgresPlainStorageSettings plainStorageSettings)
        {
            this.assignmentsStorage = assignmentsStorage;
            this.plainStorageSettings = plainStorageSettings;
        }

        class CounterObject
        {
            public int InterviewsCount { get; set; }
            public InterviewStatus Status { get; set; }
            public DateTime StatusDate { get; set; }
        }

        public async Task<CountDaysOfInterviewInStatusRow[]> LoadAsync(CountDaysOfInterviewInStatusInputModel input)
        {
            var order = input.Orders.FirstOrDefault();
            if (order == null) throw new ArgumentNullException(nameof(order));

            var rows = CreateResultSetWithPredifinedRanges();

            var datesAndStatuses = await ExecuteQueryForInterviewsStatistics(input);

            foreach (var counterObject in datesAndStatuses.AsQueryable())
            {
                var selectedRange = rows.Find(r => (!r.StartDate.HasValue || r.StartDate <= counterObject.StatusDate) && counterObject.StatusDate < r.EndDate.AddDays(1));

                if (selectedRange == null)
                    continue;

                switch (counterObject.Status)
                {
                    case InterviewStatus.Completed:
                        selectedRange.CompletedCount += counterObject.InterviewsCount;
                        break;
                    case InterviewStatus.ApprovedBySupervisor:
                        selectedRange.ApprovedBySupervisorCount += counterObject.InterviewsCount;
                        break;
                    case InterviewStatus.RejectedBySupervisor:
                        selectedRange.RejectedBySupervisorCount += counterObject.InterviewsCount;
                        break;
                    case InterviewStatus.ApprovedByHeadquarters:
                        selectedRange.ApprovedByHeadquartersCount += counterObject.InterviewsCount;
                        break;
                    case InterviewStatus.RejectedByHeadquarters:
                        selectedRange.RejectedByHeadquartersCount += counterObject.InterviewsCount;
                        break;
                }
            }

            var assignmentsDatesAndCountsForInterviewers = ExecuteQueryForAssignmentsStatistics(input, UserRoles.Interviewer.ToUserId());
            foreach (var counterObject in assignmentsDatesAndCountsForInterviewers.AsQueryable())
            {
                var selectedRange = rows.Find(r => (!r.StartDate.HasValue || r.StartDate <= counterObject.StatusDate) && counterObject.StatusDate < r.EndDate.AddDays(1));
                if (selectedRange == null)
                    continue;

                selectedRange.InterviewerAssignedCount += counterObject.InterviewsCount;
            }

            var assignmentsDatesAndCountsForSupervisors = ExecuteQueryForAssignmentsStatistics(input, UserRoles.Supervisor.ToUserId());
            foreach (var counterObject in assignmentsDatesAndCountsForSupervisors.AsQueryable())
            {
                var selectedRange = rows.Find(r => (!r.StartDate.HasValue || r.StartDate <= counterObject.StatusDate) && counterObject.StatusDate < r.EndDate.AddDays(1));
                if (selectedRange == null)
                    continue;
                selectedRange.SupervisorAssignedCount += counterObject.InterviewsCount;
            }

            return order.Direction == OrderDirection.Desc
                ? rows.OrderBy(r => r.DaysCountStart).ToArray()
                : rows.OrderByDescending(r => r.DaysCountStart).ToArray();
        }

        private static List<CountDaysOfInterviewInStatusRow> CreateResultSetWithPredifinedRanges()
        {
            var utcNow = DateTime.UtcNow;
            var rows = new List<CountDaysOfInterviewInStatusRow>();
            var addRowWithRange = new Action<int, int?>((daysStart, daysEnd) =>
            {
                rows.Add(new CountDaysOfInterviewInStatusRow()
                {
                    DaysCountStart = daysStart,
                    DaysCountEnd = daysEnd,
                    StartDate = daysEnd.HasValue ? utcNow.AddDays(-daysEnd.Value).Date : (DateTime?)null,
                    EndDate = utcNow.AddDays(-daysStart).Date
                });
            });
            addRowWithRange(1, 1);
            addRowWithRange(2, 2);
            addRowWithRange(3, 3);
            addRowWithRange(4, 4);
            addRowWithRange(5, 9);
            addRowWithRange(10, 19);
            addRowWithRange(20, 29);
            addRowWithRange(30, null);
            return rows;
        }

        private IEnumerable<CounterObject> ExecuteQueryForAssignmentsStatistics(CountDaysOfInterviewInStatusInputModel input, Guid userRoleId)
        {
            return assignmentsStorage.Query(_ =>
            {
                var filteredAssignments = _;

                if (input.TemplateId.HasValue && input.TemplateVersion.HasValue)
                {
                    filteredAssignments = filteredAssignments.Where(x
                        => x.QuestionnaireId.QuestionnaireId == input.TemplateId.Value
                           && x.QuestionnaireId.Version == input.TemplateVersion.Value);
                }

                var statusWithTime = (from f in filteredAssignments
                    where f.Quantity.HasValue && f.Responsible.RoleIds.First() == userRoleId
                    select new 
                    {
                        StatusDate = f.CreatedAtUtc.Date,
                        InterviewsCount = f.Quantity ?? 0,
                        InterviewSummariesCount = f.InterviewSummaries.Count(),
                    }
                );

                var groupedByDateStatusWithTime = (from f in statusWithTime
                    group f by new { f.StatusDate } into g
                    select new CounterObject()
                    {
                        StatusDate = g.Key.StatusDate,
                        InterviewsCount = g.Sum(a => a.InterviewsCount - a.InterviewSummariesCount),
                    }
                );

                return groupedByDateStatusWithTime;
            });
        }

        private async Task<IEnumerable<CounterObject>> ExecuteQueryForInterviewsStatistics(CountDaysOfInterviewInStatusInputModel input)
        {
            string query = GetSqlQueryForInterviews();

            IEnumerable<CounterObject> datesAndStatuses;
            using (var connection = new NpgsqlConnection(plainStorageSettings.ConnectionString))
            {
                datesAndStatuses = await connection.QueryAsync<CounterObject>(query, new
                {
                    questionnaireid = input.TemplateId,
                    questionnaireversion = input.TemplateVersion,
                });
            }
            return datesAndStatuses;
        }

        private static string GetSqlQueryForInterviews()
        {
            var assembly = typeof(CountDaysOfInterviewInStatusReport).Assembly;
            using (Stream stream = assembly.GetManifestResourceStream("WB.Core.BoundedContexts.Headquarters.Views.Reposts.Factories.CountDaysOfInterviewInStatusReport.sql"))
            using (StreamReader reader = new StreamReader(stream))
            {
                string query = reader.ReadToEnd();
                return query;
            }
        }

        public async Task<ReportView> GetReportAsync(CountDaysOfInterviewInStatusInputModel model)
        {
            var view = await this.LoadAsync(model);

            return new ReportView
            {
                Headers = new[]
                {
                    Report.COLUMN_DAYS, Report.COLUMN_INTERVIEWER_ASSIGNED, Report.COLUMN_COMPLETED,
                    Report.COLUMN_REJECTED_BY_SUPERVISOR, Report.COLUMN_APPROVED_BY_SUPERVISOR
                },
                Data = view.Select(x => new object[]
                {
                    x.DaysCountStart, x.InterviewerAssignedCount, x.CompletedCount, x.RejectedBySupervisorCount,
                    x.ApprovedBySupervisorCount
                }).ToArray()
            };
        }
    }
}