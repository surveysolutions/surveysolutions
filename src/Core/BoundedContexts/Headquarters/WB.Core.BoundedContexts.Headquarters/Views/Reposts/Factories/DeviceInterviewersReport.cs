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
using WB.Core.GenericSubdomains.Portable;
using WB.Infrastructure.Native.Storage.Postgre;

namespace WB.Core.BoundedContexts.Headquarters.Views.Reposts.Factories
{
    public class DeviceInterviewersReport : IDeviceInterviewersReport
    {
        private readonly PostgresPlainStorageSettings plainStorageSettings;
        private readonly IInterviewerVersionReader interviewerVersionReader;

        public DeviceInterviewersReport(PostgresPlainStorageSettings plainStorageSettings,
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
                throw new ArgumentException("Invalid order by column passed", nameof(order));
            }

            var targetInterviewerVersion = interviewerVersionReader.Version;

            var sql = GetSqlTexts();
            var fullQuery = string.Format(sql.query, order.ToSqlOrderBy());

            using (var connection = new NpgsqlConnection(plainStorageSettings.ConnectionString))
            {
                var rows = await connection.QueryAsync<DeviceInterviewersReportLine>(fullQuery, new
                {
                    latestAppBuildVersion = targetInterviewerVersion,
                    neededFreeStorageInBytes = InterviewerIssuesConstants.LowMemoryInBytesSize,
                    minutesMismatch = InterviewerIssuesConstants.MinutesForWrongTime,
                    targetAndroidSdkVersion = InterviewerIssuesConstants.MinAndroidSdkVersion,
                    limit = input.PageSize,
                    offset = input.PageSize * (input.Page),
                    filter = input.Filter + "%"
                });
                int totalCount = await connection.ExecuteScalarAsync<int>(sql.countQuery, new {filter = input.Filter + "%" });
                var totalRow = await GetTotalLine(fullQuery, connection);

                return new DeviceInterviewersReportView
                {
                    Items = rows,
                    TotalRow = totalRow,
                    TotalCount = totalCount
                };
            }
        }

        private async Task<DeviceInterviewersReportLine> GetTotalLine(string sql, IDbConnection connection)
        {
            string summarySql = $@"SELECT SUM(report.NeverSynchedCount) as NeverSynchedCount,
                                          SUM(OutdatedCount) as OutdatedCount,
                                          SUM(LowStorageCount) as LowStorageCount,
                                          SUM(WrongDateOnTabletCount) as WrongDateOnTabletCount,
                                          SUM(OldAndroidCount) as OldAndroidCount,
                                          SUM (NeverUploadedCount) as NeverUploadedCount,
                                          SUM(ReassignedCount) as ReassignedCount,
                                          SUM(NoQuestionnairesCount) as NoQuestionnairesCount
                                   FROM ({sql}) as report";
            var row = await connection.QueryAsync<DeviceInterviewersReportLine>(summarySql, new
            {
                latestAppBuildVersion = 15,
                neededFreeStorageInBytes = InterviewerIssuesConstants.LowMemoryInBytesSize,
                minutesMismatch = InterviewerIssuesConstants.MinutesForWrongTime,
                targetAndroidSdkVersion = InterviewerIssuesConstants.MinAndroidSdkVersion,
                limit = (int?) null,
                offset = 0,
                filter = "%"
            });

            return row.FirstOrDefault() ?? new DeviceInterviewersReportLine();
        }

        private (string query, string countQuery) GetSqlTexts()
        {
            string query;
            string countQuery;
            var assembly = typeof(DeviceInterviewersReport).Assembly;
            using (Stream stream = assembly.GetManifestResourceStream("WB.Core.BoundedContexts.Headquarters.Views.Reposts.Factories.DeviceInterviewersReport.sql"))
            using (StreamReader reader = new StreamReader(stream))
            {
                query = reader.ReadToEnd();
            }
            using (Stream stream = assembly.GetManifestResourceStream("WB.Core.BoundedContexts.Headquarters.Views.Reposts.Factories.DeviceInterviewersReportCount.sql"))
            using (StreamReader reader = new StreamReader(stream))
            {
                countQuery = reader.ReadToEnd();
            }

            return (query, countQuery);
        }

        public async Task<ReportView> GetReportAsync(DeviceByInterviewersReportInputModel input)
        {
            var view = await this.LoadAsync(input);

            return new ReportView
            {
                Headers = new[]
                {
                    Report.COLUMN_TEAMS, Report.COLUMN_NEVER_SYNCHED, Report.COLUMN_OLD_VERSION, Report.COLUMN_LESS_THAN_100MB_FREE_SPACE, Report.COLUMN_WRONG_TIME_ON_TABLET,
                    Report.COLUMN_ANDROID_4_4_OR_LOWER, Report.COLUMN_NO_ASSIGNMENTS_RECEIVED, Report.COLUMN_NEVER_UPLOADED, Report.COLUMN_TABLET_REASSIGNED
                },
                Data = view.Items.Select(x => new object[]
                {
                    x.TeamName, x.NeverSynchedCount, x.OutdatedCount, x.LowStorageCount, x.WrongDateOnTabletCount,
                    x.OldAndroidCount, x.NoQuestionnairesCount, x.NeverUploadedCount, x.ReassignedCount
                }).ToArray()
            };
        }
    }
}