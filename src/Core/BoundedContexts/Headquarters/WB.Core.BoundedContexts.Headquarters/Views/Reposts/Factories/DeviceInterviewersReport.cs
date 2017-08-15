using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Resources;
using System.Threading.Tasks;
using Dapper;
using NHibernate;
using Ninject;
using Npgsql;
using WB.Core.BoundedContexts.Headquarters.OwinSecurity;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.BoundedContexts.Headquarters.Views.Reposts.Views;
using WB.Core.BoundedContexts.Headquarters.Views.User;
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

        public async Task<DeviceInterviewersReportView> LoadAsync(string filter, int pageNumber, int pageSize)
        {
            var targetInterviewerVersion = interviewerVersionReader.Version;

            using (var connection = new NpgsqlConnection(plainStorageSettings.ConnectionString))
            {
                var rows = await connection.QueryAsync<DeviceInterviewersReportLine>(GetSqlText(), new
                {
                    latestAppBuildVersion = targetInterviewerVersion,
                    neededFreeStorageInBytes = 100 * 1024,
                    minutesMismatch = 15,
                    targetAndroidSdkVersion = 20,
                    limit = pageSize,
                    offset = pageSize * (pageNumber - 1)
                });

                return new DeviceInterviewersReportView
                {
                    Items = rows
                };
            }
        }

        private string GetSqlText()
        {
            var resourceName = "WB.Core.BoundedContexts.Headquarters.Views.Reposts.Factories.DeviceInterviewersReport.sql";

            using (Stream stream = typeof(DeviceInterviewersReport).Assembly.GetManifestResourceStream(resourceName))
            using (StreamReader reader = new StreamReader(stream))
            {
                string result = reader.ReadToEnd();
                return result;
            }
        }

       
    }
}