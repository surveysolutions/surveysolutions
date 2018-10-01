using System;
using FluentAssertions;
using Moq;
using NHibernate;
using Npgsql;
using NUnit.Framework;
using WB.Core.BoundedContexts.Headquarters.EventHandler;
using WB.Core.BoundedContexts.Headquarters.Implementation.Factories;
using WB.Core.BoundedContexts.Headquarters.Mappings;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.Transactions;
using WB.Infrastructure.Native.Storage.Postgre.Implementation;
using WB.Tests.Integration.PostgreSQLTests;

namespace WB.Tests.Integration.OldschoolChartStatisticsDataProviderTests
{
    internal class when_count_of_interviews_is_zero: with_postgres_db
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            var sessionFactory = IntegrationCreate.SessionFactory(ConnectionStringBuilder.ConnectionString, new[] { typeof(CumulativeReportStatusChangeMap) }, true);
            postgresTransactionManager = new CqrsPostgresTransactionManager(sessionFactory ?? Mock.Of<ISessionFactory>());

            pgSqlConnection = new NpgsqlConnection(ConnectionStringBuilder.ConnectionString);
            pgSqlConnection.Open();

            cumulativeReportStatusChangeStorage = new PostgreReadSideStorage<CumulativeReportStatusChange>(postgresTransactionManager, Mock.Of<ILogger>());

            oldschoolChartStatisticsDataProvider = new OldschoolChartStatisticsDataProvider(cumulativeReportStatusChangeStorage);
            BecauseOf();
        }

        [OneTimeTearDown]
        public void TearDown()
        {
            pgSqlConnection.Close();
        }

        public void BecauseOf() => result= postgresTransactionManager.ExecuteInQueryTransaction(()=>oldschoolChartStatisticsDataProvider.GetStatisticsInOldFormat(Guid.NewGuid(), 1));

        [NUnit.Framework.Test] public void should_return_null () => result.Should().BeNull();

        private static OldschoolChartStatisticsDataProvider oldschoolChartStatisticsDataProvider;
        private static StatisticsGroupedByDateAndTemplate result;
        private static CqrsPostgresTransactionManager postgresTransactionManager;
        static NpgsqlConnection pgSqlConnection;
        private static PostgreReadSideStorage<CumulativeReportStatusChange> cumulativeReportStatusChangeStorage;
    }
}
