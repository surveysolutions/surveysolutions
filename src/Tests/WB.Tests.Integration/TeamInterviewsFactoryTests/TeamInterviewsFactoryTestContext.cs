using System;
using Moq;
using NUnit.Framework;
using WB.Core.BoundedContexts.Headquarters.Mappings;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Core.BoundedContexts.Headquarters.Views.Interviews;
using WB.Core.GenericSubdomains.Portable.ServiceLocation;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Infrastructure.Native.Storage.Postgre;
using WB.Infrastructure.Native.Storage.Postgre.Implementation;
using WB.Tests.Integration.PostgreSQLEventStoreTests;

namespace WB.Tests.Integration.TeamInterviewsFactoryTests
{
    internal class TeamInterviewsFactoryTestContext 
    {
        public ITeamInterviewsFactory CreateTeamInterviewsFactory(
            out PostgreReadSideStorage<InterviewSummary> reader,
            out PostgreReadSideStorage<QuestionAnswer> featuredQuestionAnswersReader)
        {
            connectionString = DatabaseTestInitializer.InitializeDb(DbType.ReadSide);

            var sessionFactory = IntegrationCreate.SessionFactory(connectionString, new[]
            {
                typeof(InterviewSummaryMap), typeof(TimeSpanBetweenStatusesMap), typeof(QuestionAnswerMap), typeof(InterviewCommentedStatusMap)
            }, true, schemaName: "readside");

            UnitOfWork = IntegrationCreate.UnitOfWork(sessionFactory);
                
            reader = new PostgreReadSideStorage<InterviewSummary>(UnitOfWork, Mock.Of<ILogger>(), Mock.Of<IServiceLocator>());
            featuredQuestionAnswersReader = new PostgreReadSideStorage<QuestionAnswer>(UnitOfWork, Mock.Of<ILogger>(), Mock.Of<IServiceLocator>());

            return new TeamInterviewsFactory(reader);
        }
        
        protected static void ExecuteInCommandTransaction(Action action)
        {
            action();
        }


        [OneTimeTearDown]
        public void TearDown()
        {
            UnitOfWork.AcceptChanges();
            UnitOfWork.Dispose();
            DatabaseTestInitializer.DropDb(connectionString);
        }

        private string connectionString;
        protected IUnitOfWork UnitOfWork;
    }
}
