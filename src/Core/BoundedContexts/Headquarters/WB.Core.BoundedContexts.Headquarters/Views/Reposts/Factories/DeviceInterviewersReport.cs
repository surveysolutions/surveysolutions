using System;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Npgsql;
using WB.Core.BoundedContexts.Headquarters.Resources;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.BoundedContexts.Headquarters.Views.Reposts.InputModels;
using WB.Core.BoundedContexts.Headquarters.Views.Reposts.Views;
using WB.Core.BoundedContexts.Headquarters.Views.User;
using WB.Infrastructure.Native.Storage.Postgre;

namespace WB.Core.BoundedContexts.Headquarters.Views.Reposts.Factories
{
    public class DeviceInterviewersReport : IDeviceInterviewersReport
    {
        private readonly UnitOfWorkConnectionSettings plainStorageSettings;
        private readonly IInterviewerVersionReader interviewerVersionReader;

        public DeviceInterviewersReport(UnitOfWorkConnectionSettings plainStorageSettings,
            IInterviewerVersionReader interviewerVersionReader)
        {
            this.plainStorageSettings = plainStorageSettings;
            this.interviewerVersionReader = interviewerVersionReader;
        }

        public async Task<DeviceInterviewersReportView> LoadAsync(DeviceByInterviewersReportInputModel input)
        {
            if (input == null) throw new ArgumentNullException(nameof(input));

            var order = input.Orders.FirstOrDefault();
            if (order == null) throw new ArgumentNullException(nameof(order));

            if (!order.IsSortedByOneOfTheProperties(typeof(DeviceInterviewersReportLine)))
            {
                throw new ArgumentException(@"Invalid order by column passed", nameof(order));
            }

            var targetInterviewerVersion = interviewerVersionReader.Version;

            var sql = GetSqlTexts();
            var fullQuery = string.Format(sql, order.ToSqlOrderBy());

            using (var connection = new NpgsqlConnection(plainStorageSettings.ConnectionString))
            {
                var rows = await connection.QueryAsync<DeviceInterviewersReportLine>(fullQuery, new
                {
                    latestAppBuildVersion = targetInterviewerVersion,
                    neededFreeStorageInBytes = InterviewerIssuesConstants.LowMemoryInBytesSize,
                    minutesMismatch = InterviewerIssuesConstants.MinutesForWrongTime,
                    targetAndroidSdkVersion = InterviewerIssuesConstants.MinAndroidSdkVersion,
                    limit = input.PageSize,
                    offset = input.Page,
                    filter = input.Filter + "%"
                });
                int totalCount = await GetTotalRowsCountAsync(fullQuery, targetInterviewerVersion, input, connection);
                var totalRow = await GetTotalLine(fullQuery, targetInterviewerVersion, input.Filter, connection);

                return new DeviceInterviewersReportView
                {
                    Items = rows,
                    TotalCount = totalCount,
                    TotalRow = totalRow
                };
            }
        }

        private async Task<int> GetTotalRowsCountAsync(string sql, int? targetInterviewerVersion, DeviceByInterviewersReportInputModel input, IDbConnection connection)
        {
            string summarySql = $@"SELECT COUNT(*) FROM ({sql}) as report";

            var row = await connection.ExecuteScalarAsync<int>(summarySql, new
            {
                latestAppBuildVersion = targetInterviewerVersion,
                neededFreeStorageInBytes = InterviewerIssuesConstants.LowMemoryInBytesSize,
                minutesMismatch = InterviewerIssuesConstants.MinutesForWrongTime,
                targetAndroidSdkVersion = InterviewerIssuesConstants.MinAndroidSdkVersion,
                limit = (int?) null,
                offset = 0,
                filter = input.Filter + "%"
            });

            return row;
        }

        private async Task<DeviceInterviewersReportLine> GetTotalLine(string sql, int? targetInterviewerVersion, string filter, IDbConnection connection)
        {
            string summarySql = $@"SELECT SUM(report.NeverSynchedCount) as NeverSynchedCount,
                                          SUM(report.OutdatedCount) as OutdatedCount,
                                          SUM(report.LowStorageCount) as LowStorageCount,
                                          -- SUM(report.WrongDateOnTabletCount) as WrongDateOnTabletCount,
                                          SUM(report.OldAndroidCount) as OldAndroidCount,
                                          SUM(report.NeverUploadedCount) as NeverUploadedCount,
                                          SUM(report.ReassignedCount) as ReassignedCount,
                                          SUM(report.NoQuestionnairesCount) as NoQuestionnairesCount
                                   FROM ({sql}) as report";

            var row = await connection.QueryAsync<DeviceInterviewersReportLine>(summarySql, new
            {
                latestAppBuildVersion = targetInterviewerVersion,
                neededFreeStorageInBytes = InterviewerIssuesConstants.LowMemoryInBytesSize,
                minutesMismatch = InterviewerIssuesConstants.MinutesForWrongTime,
                targetAndroidSdkVersion = InterviewerIssuesConstants.MinAndroidSdkVersion,
                limit = (int?) null,
                offset = 0,
                filter = filter + "%"
            });

            var result = row.FirstOrDefault() ?? new DeviceInterviewersReportLine();
            result.TeamName = Strings.AllTeams;
            return result;
        }

        private static string DeviceInterviewersReportSql = null;

        private string GetSqlTexts()
        {
            if (DeviceInterviewersReportSql != null) return DeviceInterviewersReportSql;

            var assembly = typeof(DeviceInterviewersReport).Assembly;
            using (Stream stream = assembly.GetManifestResourceStream("WB.Core.BoundedContexts.Headquarters.Views.Reposts.Factories.DeviceInterviewersReport.sql"))
            using (StreamReader reader = new StreamReader(stream))
            {
                DeviceInterviewersReportSql = reader.ReadToEnd();
            }
            return DeviceInterviewersReportSql;
        }

        public async Task<ReportView> GetReportAsync(DeviceByInterviewersReportInputModel input)
        {
            var view = await this.LoadAsync(input);

            return new ReportView
            {
                Headers = new[]
                {
                    Report.COLUMN_TEAMS,
                    Report.COLUMN_NEVER_SYNCHED,
                    Report.COLUMN_NO_ASSIGNMENTS_RECEIVED,
                    Report.COLUMN_NEVER_UPLOADED,
                    Report.COLUMN_TABLET_REASSIGNED,
                    Report.COLUMN_OLD_VERSION,
                    Report.COLUMN_ANDROID_4_4_OR_LOWER,
                    // Report.COLUMN_WRONG_TIME_ON_TABLET,
                    Report.COLUMN_LESS_THAN_100MB_FREE_SPACE
                },
                Data = new[]
                {
                    new object[]
                    {
                        view.TotalRow.TeamName,
                        view.TotalRow.NeverSynchedCount,
                        view.TotalRow.NoQuestionnairesCount,
                        view.TotalRow.NeverUploadedCount,
                        view.TotalRow.ReassignedCount,
                        view.TotalRow.OutdatedCount,
                        view.TotalRow.OldAndroidCount,
                        // view.TotalRow.WrongDateOnTabletCount,
                        view.TotalRow.LowStorageCount
                    }
                }.Concat(view.Items.Select(x => new object[]
                {
                    x.TeamName,
                    x.NeverSynchedCount,
                    x.NoQuestionnairesCount,
                    x.NeverUploadedCount,
                    x.ReassignedCount,
                    x.OutdatedCount,
                    x.OldAndroidCount,
                    // x.WrongDateOnTabletCount,
                    x.LowStorageCount
                })).ToArray()
            };
        }
    }
}
