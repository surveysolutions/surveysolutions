using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NHibernate;
using Npgsql;
using NUnit.Framework;
using WB.Core.BoundedContexts.Headquarters.Assignments;
using WB.Core.BoundedContexts.Headquarters.Mappings;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.Infrastructure.Transactions;
using WB.Core.SharedKernels.DataCollection;
using WB.Infrastructure.Native.Storage.Postgre.Implementation;
using WB.Tests.Abc;
using WB.Tests.Integration.PostgreSQLEventStoreTests;

namespace WB.Tests.Integration.AssignmentsDeletionServiceTests
{
    [TestFixture]
    public class AssignmetnsDeletionServiceTests 
    {
        private ISessionFactory sessionFactory;
        private PlainPostgresTransactionManager plainPostgresTransactionManager;
        private string connectionString;

        [OneTimeSetUp]
        public void OneTimeSetup()
        {
            connectionString = DatabaseTestInitializer.InitializeDb(DbType.PlainStore);

            sessionFactory = IntegrationCreate.SessionFactory(
                this.connectionString, new List<Type>
                {
                    typeof(ProfileMap),
                    typeof(ReadonlyUserMap),
                    typeof(AssignmentMap),
                    typeof(QuestionnaireLiteViewItemMap),
                    typeof(Core.BoundedContexts.Headquarters.Assignments.InterviewSummaryMap)
                }, true, "plainstore");
            plainPostgresTransactionManager = new PlainPostgresTransactionManager(sessionFactory);
        }

        [Test]
        public async Task when_deleting_all_assignments_for_questionnaire()
        {
            Guid questionniareId = Guid.Parse("aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa");
            int questionnaireVersion = 59;

            var questionnaireIdentity = Create.Entity.QuestionnaireIdentity(questionniareId, questionnaireVersion);
            Assignment assignment = Create.Entity.Assignment(questionnaireIdentity: questionnaireIdentity);
            assignment.IdentifyingData.Add(Create.Entity.IdentifyingAnswer(assignment, Create.Entity.Identity(Guid.NewGuid())));
            assignment.IdentifyingData.Add(Create.Entity.IdentifyingAnswer(assignment, Create.Entity.Identity(Guid.NewGuid())));

            IPlainStorageAccessor<Assignment> assignments = new PostgresPlainStorageRepository<Assignment>(plainPostgresTransactionManager);

            this.plainPostgresTransactionManager.ExecuteInPlainTransaction(() => assignments.Store(assignment, null));

            var service = new AssignmetnsDeletionService(sessionFactory);

            // act 
            service.Delete(questionnaireIdentity);

            // assert
            var actual = this.plainPostgresTransactionManager.ExecuteInPlainTransaction(() => assignments.Query(_ => _.Count()));
            Assert.That(actual, Is.EqualTo(0), "All assignments should be deleted");

            using (NpgsqlConnection connection = new NpgsqlConnection(this.connectionString))
            {
                await connection.OpenAsync();
                var npgsqlCommand = connection.CreateCommand();
                npgsqlCommand.CommandText = "SELECT COUNT(*) from plainstore.assignmentsidentifyinganswers";
                var identifyingQuestionsCount = await npgsqlCommand.ExecuteScalarAsync();

                Assert.That(identifyingQuestionsCount, Is.EqualTo(0), "All identifying question answers should be removed with assignment`");
            }
        }

        [OneTimeTearDown]
        public void OneTimeTearDown()
        {
            this.plainPostgresTransactionManager.Dispose();
            this.sessionFactory.Dispose();
            DatabaseTestInitializer.DropDb(this.connectionString);
        }
    }
}