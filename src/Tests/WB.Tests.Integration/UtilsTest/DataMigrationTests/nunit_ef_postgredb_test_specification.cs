using NUnit.Framework;
using WB.Tests.Integration.PostgreSQLTests;

namespace WB.Tests.Integration.UtilsTest.DataMigrationTests
{
    public class nunit_ef_postgredb_test_specification : with_postgres_db
    {
        [OneTimeSetUp]
        public void OneTimeSetup() => Context();

        [OneTimeTearDown]
        public void TearDown() => Cleanup();

        [SetUp]
        public void Setup()
        {
            TestContext.CurrentContext.Test.Properties.Add("connection", connectionStringBuilder.ConnectionString);
        }
    }
}