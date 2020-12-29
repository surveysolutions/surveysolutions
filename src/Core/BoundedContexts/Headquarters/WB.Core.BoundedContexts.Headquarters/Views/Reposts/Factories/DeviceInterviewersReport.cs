using System;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using WB.Core.BoundedContexts.Headquarters.Resources;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.BoundedContexts.Headquarters.Views.Reposts.InputModels;
using WB.Core.BoundedContexts.Headquarters.Views.Reposts.Views;
using WB.Core.BoundedContexts.Headquarters.Views.User;
using WB.Infrastructure.Native.Storage.Postgre;
using WB.Infrastructure.Native.Workspaces;

namespace WB.Core.BoundedContexts.Headquarters.Views.Reposts.Factories
{
    public class DeviceInterviewersReport : IDeviceInterviewersReport
    {
        private const string ReportBySupervisors = "DeviceInterviewersReport";
        private const string ReportByInterviewers = "DevicesInterviewersForSupervisor";

        private readonly IUnitOfWork unitOfWork;
        private readonly IInterviewerVersionReader interviewerVersionReader;
        private readonly IUserViewFactory userViewFactory;
        private readonly IWorkspaceContextAccessor workspaceContextAccessor;

        public DeviceInterviewersReport(
            IUnitOfWork unitOfWork,
            IInterviewerVersionReader interviewerVersionReader,
            IUserViewFactory userViewFactory,
            IWorkspaceContextAccessor workspaceContextAccessor)
        {
            this.unitOfWork = unitOfWork;
            this.interviewerVersionReader = interviewerVersionReader;
            this.userViewFactory = userViewFactory;
            this.workspaceContextAccessor = workspaceContextAccessor;
        }

        public async Task<DeviceInterviewersReportView> LoadAsync(DeviceByInterviewersReportInputModel input)
        {
            if (input == null) throw new ArgumentNullException(nameof(input));

            var order = input.Orders.FirstOrDefault();
            if (order == null) throw new ArgumentNullException(nameof(order));

            var workspace = workspaceContextAccessor.CurrentWorkspace();

            if (workspace == null)
                throw new MissingWorkspaceException("Cannot execute report against missing workspace");

            if (!order.IsSortedByOneOfTheProperties(typeof(DeviceInterviewersReportLine)))
            {
                throw new ArgumentException(@"Invalid order by column passed", nameof(order));
            }

            var targetInterviewerVersion = await interviewerVersionReader.InterviewerBuildNumber();

            var sql = GetSqlTexts(input.SupervisorId.HasValue ? ReportByInterviewers : ReportBySupervisors);
            var fullQuery = string.Format(sql, order.ToSqlOrderBy());

            var connection = unitOfWork.Session.Connection;
            var rows = await connection.QueryAsync<DeviceInterviewersReportLine>(fullQuery, new
            {
                latestAppBuildVersion = targetInterviewerVersion,
                neededFreeStorageInBytes = InterviewerIssuesConstants.LowMemoryInBytesSize,
                minutesMismatch = InterviewerIssuesConstants.MinutesForWrongTime,
                targetAndroidSdkVersion = InterviewerIssuesConstants.MinAndroidSdkVersion,
                limit = input.PageSize,
                offset = input.Page,
                filter = input.Filter + "%",
                supervisorId = input.SupervisorId,
                workspace = workspace.Name
            });

            int totalCount = await GetTotalRowsCountAsync(fullQuery, targetInterviewerVersion, input, connection, workspace);
            var totalRow = await GetTotalLine(fullQuery, input.SupervisorId, targetInterviewerVersion, input.Filter,
                connection, workspace);

            return new DeviceInterviewersReportView
            {
                Items = rows,
                TotalCount = totalCount,
                TotalRow = totalRow
            };
        }

        private async Task<int> GetTotalRowsCountAsync(string sql, int? targetInterviewerVersion,
            DeviceByInterviewersReportInputModel input, IDbConnection connection, WorkspaceContext workspace)
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
                filter = input.Filter + "%",
                supervisorId = input.SupervisorId,
                workspace = workspace.Name
            });

            return row;
        }

        private async Task<DeviceInterviewersReportLine> GetTotalLine(string sql, Guid? supervisorId,
            int? targetInterviewerVersion, string filter, IDbConnection connection, WorkspaceContext workspace)
        {
            string summarySql = $@"SELECT SUM(report.NeverSynchedCount) as NeverSynchedCount,
                                          SUM(report.OutdatedCount) as OutdatedCount,
                                          SUM(report.OldAndroidCount) as OldAndroidCount,
                                          SUM(report.NeverUploadedCount) as NeverUploadedCount,
                                          SUM(report.ReassignedCount) as ReassignedCount,
                                          SUM(report.NoQuestionnairesCount) as NoQuestionnairesCount,
                                          SUM(report.TeamSize) as TeamSize
                                   FROM ({sql}) as report";

            var row = await connection.QueryAsync<DeviceInterviewersReportLine>(summarySql, new
            {
                latestAppBuildVersion = targetInterviewerVersion,
                neededFreeStorageInBytes = InterviewerIssuesConstants.LowMemoryInBytesSize,
                minutesMismatch = InterviewerIssuesConstants.MinutesForWrongTime,
                targetAndroidSdkVersion = InterviewerIssuesConstants.MinAndroidSdkVersion,
                limit = (int?) null,
                offset = 0,
                filter = filter + "%",
                supervisorId = supervisorId,
                workspace = workspace.Name
            });

            var result = row.FirstOrDefault() ?? new DeviceInterviewersReportLine();
            result.TeamName = supervisorId.HasValue ? this.userViewFactory.GetUser(new UserViewInputModel(supervisorId.Value)).UserName : Strings.AllTeams;
            return result;
        }

        private string GetSqlTexts(string reportName)
        {
            string result = null;
            var assembly = typeof(DeviceInterviewersReport).Assembly;
            var resourceName = $"WB.Core.BoundedContexts.Headquarters.Views.Reposts.Factories.{reportName}.sql";
            using Stream stream = assembly.GetManifestResourceStream(resourceName);
            using StreamReader reader = new StreamReader(stream);
            result = reader.ReadToEnd();
            return result;
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
                    Report.COLUMN_TEAM_SIZE
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
                        view.TotalRow.TeamSize
                    }
                }.Concat(view.Items.Select(x => new object[]
                {
                    x.TeamName,
                    x.NeverSynchedCount,
                    x.NoQuestionnairesCount,
                    x.NeverUploadedCount,
                    x.ReassignedCount,
                    x.OutdatedCount,
                    x.TeamSize
                })).ToArray()
            };
        }
    }
}
