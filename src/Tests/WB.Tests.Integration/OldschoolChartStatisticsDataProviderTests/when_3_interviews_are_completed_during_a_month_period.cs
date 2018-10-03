using FluentAssertions;
using Moq;
using Npgsql;
using NUnit.Framework;
using System;
using WB.Core.BoundedContexts.Headquarters.EventHandler;
using WB.Core.BoundedContexts.Headquarters.Implementation.Factories;
using WB.Core.BoundedContexts.Headquarters.Mappings;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Core.GenericSubdomains.Portable.ServiceLocation;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Infrastructure.Native.Storage.Postgre;
using WB.Infrastructure.Native.Storage.Postgre.Implementation;
using WB.Tests.Integration.PostgreSQLTests;

namespace WB.Tests.Integration.OldschoolChartStatisticsDataProviderTests
{
    internal class when_3_interviews_are_completed_during_a_month_period : with_postgres_db
    {
        [NUnit.Framework.OneTimeSetUp]
        public void context()
        {
            var sessionFactory = IntegrationCreate.SessionFactory(ConnectionStringBuilder.ConnectionString,
                new[] { typeof(CumulativeReportStatusChangeMap) }, true);
            UnitOfWork = Mock.Of<IUnitOfWork>(x => x.Session == sessionFactory.OpenSession());
            
            cumulativeReportStatusChangeStorage =
                new PostgreReadSideStorage<CumulativeReportStatusChange>(UnitOfWork,
                    Mock.Of<ILogger>(), Mock.Of<IServiceLocator>());

            var cumulativeReportStatusChangeBegin =
                IntegrationCreate.CumulativeReportStatusChange(questionnaireId, questionnaireVersion, beginDate);
            var cumulativeReportStatusChangeInBetween =
                IntegrationCreate.CumulativeReportStatusChange(questionnaireId, questionnaireVersion, dateInBetween);
            var cumulativeReportStatusChangeEnd =
                IntegrationCreate.CumulativeReportStatusChange(questionnaireId, questionnaireVersion, endDate);
            cumulativeReportStatusChangeStorage.Store(cumulativeReportStatusChangeBegin,
                cumulativeReportStatusChangeBegin.EntryId);
            cumulativeReportStatusChangeStorage.Store(cumulativeReportStatusChangeInBetween,
                cumulativeReportStatusChangeInBetween.EntryId);
            cumulativeReportStatusChangeStorage.Store(cumulativeReportStatusChangeEnd,
                cumulativeReportStatusChangeEnd.EntryId);

            oldschoolChartStatisticsDataProvider = new OldschoolChartStatisticsDataProvider(cumulativeReportStatusChangeStorage);
            BecauseOf();
        }

        [OneTimeTearDown]
        public void TearDown()
        {
            UnitOfWork.Dispose();
        }

        private void BecauseOf() =>
                result = oldschoolChartStatisticsDataProvider.GetStatisticsInOldFormat(questionnaireId, questionnaireVersion);

        [NUnit.Framework.Test] public void should_return_32_days_timespan() => result.StatisticsByDate.Count.Should().Be(32);


        [NUnit.Framework.Test] public void should_return_1_completed_interview_on_first_day() => 
            result.StatisticsByDate[beginDate].CompletedCount.Should().Be(1);

        [NUnit.Framework.Test] public void should_return_2_completed_interview_on_april_first() => 
            result.StatisticsByDate[dateInBetween].CompletedCount.Should().Be(2);

        [NUnit.Framework.Test] public void should_return_3_completed_interview_on_last_day() => 
            result.StatisticsByDate[endDate].CompletedCount.Should().Be(3);


        private static OldschoolChartStatisticsDataProvider oldschoolChartStatisticsDataProvider;
        private static StatisticsGroupedByDateAndTemplate result;
        private static IUnitOfWork UnitOfWork;
        private static Guid questionnaireId = Guid.NewGuid();
        private static long questionnaireVersion = 1;
        private static DateTime beginDate = new DateTime(2016, 3, 18);
        private static DateTime dateInBetween = new DateTime(2016, 4, 1);
        private static DateTime endDate = new DateTime(2016, 4, 18);
        private static PostgreReadSideStorage<CumulativeReportStatusChange> cumulativeReportStatusChangeStorage;
    }
}
