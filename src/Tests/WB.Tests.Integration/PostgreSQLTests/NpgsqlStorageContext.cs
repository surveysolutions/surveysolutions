using Moq;
using NHibernate;
using Npgsql;
using WB.Core.BoundedContexts.Headquarters.Mappings;
using WB.Core.BoundedContexts.Headquarters.Views.DataExport;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Infrastructure.Native.Storage.Postgre.Implementation;

namespace WB.Tests.Integration.PostgreSQLTests
{
    internal abstract class NpgsqlStorageContext : NpgsqlTestContext
    {
        protected PostgreReadSideStorage<InterviewDataExportRecord> CreateInterviewExportRepository()
        {
            var sessionFactory = IntegrationCreate.SessionFactory(connectionStringBuilder.ConnectionString, new[] { typeof(InterviewDataExportRecordMap) }, true);
            transactionManager = new CqrsPostgresTransactionManager(sessionFactory ?? Mock.Of<ISessionFactory>());

            pgSqlConnection = new NpgsqlConnection(connectionStringBuilder.ConnectionString);
            pgSqlConnection.Open();

            return new PostgreReadSideStorage<InterviewDataExportRecord>(transactionManager, Mock.Of<ILogger>(), "RecordId");
        }
    }
}
