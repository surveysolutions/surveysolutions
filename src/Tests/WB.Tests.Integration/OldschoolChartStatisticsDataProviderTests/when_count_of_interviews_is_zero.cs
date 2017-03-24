using System;
using Machine.Specifications;
using Moq;
using NHibernate;
using Npgsql;
using WB.Core.BoundedContexts.Headquarters.EventHandler;
using WB.Core.BoundedContexts.Headquarters.Implementation.Factories;
using WB.Core.BoundedContexts.Headquarters.Mappings;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.Transactions;
using WB.Infrastructure.Native.Storage.Postgre.Implementation;
using WB.Tests.Integration.PostgreSQLTests;
using It = Machine.Specifications.It;

namespace WB.Tests.Integration.OldschoolChartStatisticsDataProviderTests
{
    internal class when_count_of_interviews_is_zero: with_postgres_db
    {
        Establish context = () =>
        {
            var sessionFactory = IntegrationCreate.SessionFactory(connectionStringBuilder.ConnectionString, new[] { typeof(CumulativeReportStatusChangeMap) });
            postgresTransactionManager = new CqrsPostgresTransactionManager(sessionFactory ?? Mock.Of<ISessionFactory>());

            pgSqlConnection = new NpgsqlConnection(connectionStringBuilder.ConnectionString);
            pgSqlConnection.Open();

            cumulativeReportStatusChangeStorage = new PostgreReadSideStorage<CumulativeReportStatusChange>(postgresTransactionManager, Mock.Of<ILogger>(), "EntryId");

            oldschoolChartStatisticsDataProvider = new OldschoolChartStatisticsDataProvider(cumulativeReportStatusChangeStorage);
        };

        Cleanup things = () => { pgSqlConnection.Close(); };

        Because of = () => result= postgresTransactionManager.ExecuteInQueryTransaction(()=>oldschoolChartStatisticsDataProvider.GetStatisticsInOldFormat(Guid.NewGuid(), 1));

        It should_return_null =
            () => result.ShouldBeNull();

        private static OldschoolChartStatisticsDataProvider oldschoolChartStatisticsDataProvider;
        private static StatisticsGroupedByDateAndTemplate result;
        private static CqrsPostgresTransactionManager postgresTransactionManager;
        static NpgsqlConnection pgSqlConnection;
        private static PostgreReadSideStorage<CumulativeReportStatusChange> cumulativeReportStatusChangeStorage;
    }
}