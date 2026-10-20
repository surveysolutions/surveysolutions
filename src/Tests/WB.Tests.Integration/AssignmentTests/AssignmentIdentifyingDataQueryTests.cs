using System;
using System.Collections.Generic;
using System.Linq;
using NHibernate;
using NUnit.Framework;
using WB.Core.BoundedContexts.Headquarters.Assignments;
using WB.Core.BoundedContexts.Headquarters.Mappings;
using WB.Core.BoundedContexts.Headquarters.Users.Providers;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Core.BoundedContexts.Headquarters.Views.Questionnaire;
using WB.Infrastructure.Native.Storage;
using WB.Infrastructure.Native.Storage.Postgre;
using WB.Tests.Abc;
using WB.Tests.Integration.PostgreSQLEventStoreTests;

namespace WB.Tests.Integration.AssignmentTests
{
    [TestFixture]
    public class AssignmentIdentifyingDataQueryTests
    {
        private string connectionString;
        private ISessionFactory sessionFactory;
        private IUnitOfWork unitOfWork;

        [OneTimeSetUp]
        public void OneTimeSetup()
        {
            SetUp.MockedServiceLocator();
            connectionString = DatabaseTestInitializer.CreateAndInitializeDb(DbType.ReadSide, DbType.Users);
            var workspace = Create.Service.WorkspaceContextAccessor();
            sessionFactory = IntegrationCreate.SessionFactory(connectionString, new List<Type>
            {
                typeof(AssignmentMap),
                typeof(AssignmentGpsMap),
                typeof(InterviewSummaryMap),
                typeof(InterviewGpsMap),
                typeof(QuestionnaireCompositeItemMap),
                typeof(IdentifyEntityValueMap),
                typeof(InterviewStatisticsReportRowMap),
                typeof(TimeSpanBetweenStatusesMap),
                typeof(CumulativeReportStatusChangeMap),
                typeof(InterviewCommentedStatusMap),
                typeof(InterviewCommentMap),
                typeof(QuestionnaireBrowseItemMap),
                typeof(ReadonlyUserMap),
                typeof(WorkspaceUserProfileMap)
            }, true, workspace.CurrentWorkspace().SchemaName);
        }

        [SetUp]
        public void Setup()
        {
            unitOfWork = IntegrationCreate.UnitOfWork(sessionFactory);
        }

        [TearDown]
        public void Teardown()
        {
            unitOfWork.Dispose();
        }

        [OneTimeTearDown]
        public void OneTimeTeardown()
        {
            sessionFactory.Dispose();
            DatabaseTestInitializer.DropDb(connectionString);
        }

        [Test]
        public void should_filter_assignment_identifying_data_by_mapped_question_id()
        {
            var matching = Create.Entity.Assignment(
                id: 1,
                identifyingAnswers: new List<IdentifyingAnswer>
                {
                    Create.Entity.IdentifyingAnswer(identity: Create.Identity(Id.gA), answer: "yes", answerAsString: "Yes")
                });
            var differentQuestion = Create.Entity.Assignment(
                id: 2,
                identifyingAnswers: new List<IdentifyingAnswer>
                {
                    Create.Entity.IdentifyingAnswer(identity: Create.Identity(Id.gB), answer: "yes", answerAsString: "Yes")
                });

            unitOfWork.Session.Save(matching);
            unitOfWork.Session.Save(differentQuestion);
            unitOfWork.Session.Flush();

            var assignments = unitOfWork.Session.Query<Assignment>()
                .Where(x => x.IdentifyingData.Any(a => a.Identity.Id == Id.gA && a.Answer == "yes"))
                .Select(x => x.Id)
                .ToList();

            Assert.That(assignments, Is.EqualTo(new[] { 1 }));
        }
    }
}
