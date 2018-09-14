using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Main.Core.Entities.SubEntities;
using Npgsql;
using WB.Core.BoundedContexts.Headquarters.Resources;
using WB.Core.BoundedContexts.Headquarters.Views.Reports.InputModels;
using WB.Core.BoundedContexts.Headquarters.Views.Reports.Views;
using WB.Core.BoundedContexts.Headquarters.Views.Reposts.Views;
using WB.Core.BoundedContexts.Headquarters.Views.User;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Infrastructure.Native.Storage.Postgre;


namespace WB.Core.BoundedContexts.Headquarters.Views.Reports.Factories
{
    public class StatusDurationReport : IStatusDurationReport
    {
        private readonly PostgresPlainStorageSettings plainStorageSettings;

        private const string InterviewsScriptName = "WB.Core.BoundedContexts.Headquarters.Views.Reposts.Factories.StatusDurationReportInterviews.sql";
        private const string AssignmentsScriptName = "WB.Core.BoundedContexts.Headquarters.Views.Reposts.Factories.StatusDurationReportAssignments.sql";

        public StatusDurationReport(PostgresPlainStorageSettings plainStorageSettings)
        {
            this.plainStorageSettings = plainStorageSettings;
        }

        class InterviewsCounterObject
        {
            public int InterviewsCount { get; set; }
            public InterviewStatus Status { get; set; }
            public int Days { get; set; }
        }

        class AssignmentsCounterObject
        {
            public int Count { get; set; }
            public Guid RoleId { get; set; }
            public int Days { get; set; }
        }

        public async Task<StatusDurationView> LoadAsync(StatusDurationInputModel input)
        {
            var order = input.Orders.FirstOrDefault();
            if (order == null) throw new ArgumentNullException(nameof(order));

            var rows = CreateResultSetWithPredefinedRanges();

            var datesAndStatuses = await ExecuteQueryForInterviewsStatistics(input);

            foreach (var counterObject in datesAndStatuses.AsQueryable())
            {
                var selectedRange = rows.Find(r => (!r.DaysCountEnd.HasValue || r.DaysCountEnd >= counterObject.Days) && counterObject.Days >= r.DaysCountStart);

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

            var interviewerId = UserRoles.Interviewer.ToUserId();
            var supervisorId = UserRoles.Supervisor.ToUserId();
            var assignmentsDatesAndCountsForInterviewers = await ExecuteQueryForAssignmentsStatistics(input);
            foreach (var counterObject in assignmentsDatesAndCountsForInterviewers.AsQueryable())
            {
                var selectedRange = rows.Find(r => (!r.DaysCountEnd.HasValue || r.DaysCountEnd >= counterObject.Days) && counterObject.Days >= r.DaysCountStart);
                if (selectedRange == null)
                    continue;

                if (counterObject.RoleId == interviewerId)
                    selectedRange.InterviewerAssignedCount += counterObject.Count;
                else if (counterObject.RoleId == supervisorId)
                    selectedRange.SupervisorAssignedCount += counterObject.Count;
            }

            var data = SortData(order, rows);
            SetRowHeaderForEachRecord(data);

            return new StatusDurationView
            {
                Items = data.ToArray(),
                TotalCount = data.Count,
                TotalRow = new StatusDurationRow()
                {
                    InterviewerAssignedCount = data.Sum(r => r.InterviewerAssignedCount),
                    SupervisorAssignedCount = data.Sum(r => r.SupervisorAssignedCount),
                    CompletedCount = data.Sum(r => r.CompletedCount),
                    ApprovedBySupervisorCount = data.Sum(r => r.ApprovedBySupervisorCount),
                    RejectedBySupervisorCount = data.Sum(r => r.RejectedBySupervisorCount),
                    ApprovedByHeadquartersCount = data.Sum(r => r.ApprovedByHeadquartersCount),
                    RejectedByHeadquartersCount = data.Sum(r => r.RejectedByHeadquartersCount),
                    RowHeader = Strings.AllPeriods
                }
            };
        }

        private static void SetRowHeaderForEachRecord(List<StatusDurationRow> data)
        {
            foreach (var dataRow in data)
            {
                if (dataRow.DaysCountStart == dataRow.DaysCountEnd)
                    dataRow.RowHeader = $"{dataRow.DaysCountStart}";
                else if (!dataRow.DaysCountEnd.HasValue)
                    dataRow.RowHeader = $"{dataRow.DaysCountStart}+";
                else
                    dataRow.RowHeader = $"{dataRow.DaysCountStart} - {dataRow.DaysCountEnd}";
            }
        }

        private static List<StatusDurationRow> SortData(OrderRequestItem order, List<StatusDurationRow> rows)
        {
            var data = order.Direction == OrderDirection.Desc
                ? rows.OrderBy(r => r.DaysCountStart).ToList()
                : rows.OrderByDescending(r => r.DaysCountStart).ToList();
            return data;
        }

        private static List<StatusDurationRow> CreateResultSetWithPredefinedRanges()
        {
            var utcNow = DateTime.UtcNow;
            var rows = new List<StatusDurationRow>();
            var addRowWithRange = new Action<int, int?>((daysStart, daysEnd) =>
            {
                rows.Add(new StatusDurationRow()
                {
                    DaysCountStart = daysStart,
                    DaysCountEnd = daysEnd,
                    StartDate = daysEnd.HasValue ? utcNow.AddDays(-daysEnd.Value) : (DateTime?)null,
                    EndDate = utcNow.AddDays(-daysStart + 1).AddSeconds(-1)
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

        private async Task<IEnumerable<AssignmentsCounterObject>> ExecuteQueryForAssignmentsStatistics(StatusDurationInputModel input)
        {
            string query = GetSqlQueryForInterviews(AssignmentsScriptName);

            IEnumerable<AssignmentsCounterObject> datesAndStatuses;
            using (var connection = new NpgsqlConnection(plainStorageSettings.ConnectionString))
            {
                datesAndStatuses = await connection.QueryAsync<AssignmentsCounterObject>(query, new
                {
                    questionnaireid = input.TemplateId,
                    questionnaireversion = input.TemplateVersion,
                    supervisorid = input.SupervisorId
                });
            }
            return datesAndStatuses;
        }

        private async Task<IEnumerable<InterviewsCounterObject>> ExecuteQueryForInterviewsStatistics(StatusDurationInputModel input)
        {
            string query = GetSqlQueryForInterviews(InterviewsScriptName);

            IEnumerable<InterviewsCounterObject> datesAndStatuses;
            using (var connection = new NpgsqlConnection(plainStorageSettings.ConnectionString))
            {
                datesAndStatuses = await connection.QueryAsync<InterviewsCounterObject>(query, new
                {
                    questionnaireid = input.TemplateId,
                    questionnaireversion = input.TemplateVersion,
                    supervisorid = input.SupervisorId
                });
            }
            return datesAndStatuses;
        }

        private static string GetSqlQueryForInterviews(string scriptName)
        {
            var assembly = typeof(StatusDurationReport).Assembly;
            using (Stream stream = assembly.GetManifestResourceStream(scriptName))
            using (StreamReader reader = new StreamReader(stream))
            {
                string query = reader.ReadToEnd();
                return query;
            }
        }

        public async Task<ReportView> GetReportAsync(StatusDurationInputModel model)
        {
            var view = await this.LoadAsync(model);

            return new ReportView
            {
                Headers = new[]
                {
                    Report.COLUMN_DAYS, Report.COLUMN_SUPERVISOR_ASSIGNED, Report.COLUMN_INTERVIEWER_ASSIGNED, Report.COLUMN_COMPLETED,
                    Report.COLUMN_REJECTED_BY_SUPERVISOR, Report.COLUMN_APPROVED_BY_SUPERVISOR, Report.COLUMN_REJECTED_BY_HQ, Report.COLUMN_APPROVED_BY_HQ,
                    Report.COLUMN_TOTAL
                },
                Data = new[]
                {
                    new object[]
                    {
                        view.TotalRow.RowHeader, view.TotalRow.SupervisorAssignedCount, view.TotalRow.InterviewerAssignedCount, view.TotalRow.CompletedCount,
                        view.TotalRow.RejectedBySupervisorCount, view.TotalRow.ApprovedBySupervisorCount, view.TotalRow.RejectedByHeadquartersCount, view.TotalRow.ApprovedByHeadquartersCount,
                        view.TotalRow.TotalCount
                    }
                }.Concat(view.Items.Select(x => new object[]
                {
                    x.RowHeader, x.SupervisorAssignedCount, x.InterviewerAssignedCount, x.CompletedCount,
                    x.RejectedBySupervisorCount, x.ApprovedBySupervisorCount, x.RejectedByHeadquartersCount, x.ApprovedByHeadquartersCount,
                    x.TotalCount
                })).ToArray()
            };
        }
    }
}
