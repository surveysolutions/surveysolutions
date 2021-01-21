using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using Quartz;
using WB.Core.BoundedContexts.Headquarters.Workspaces;
using WB.Core.BoundedContexts.Headquarters.Workspaces.Jobs;
using WB.Core.Infrastructure.PlainStorage;
using WB.Infrastructure.Native.Storage.Postgre;
using WB.Tests.Abc;

namespace WB.Tests.Unit.BoundedContexts.Headquarters.Workspaces
{
    [TestFixture(TestOf = typeof(DeleteWorkspaceSchemaJob))]
    public class DeleteWorkspaceSchemaJobTests
    {
        private Mock<IUnitOfWork> unitOfWork;
        private IPlainStorageAccessor<Workspace> workspaces;
        private Mock<ILogger<DeleteWorkspaceSchemaJob>> logger;
        private DeleteWorkspaceSchemaJob Subject;
        private JobDataMap jobDataMap;

        [SetUp]
        public void Setup()
        {
            this.unitOfWork = new Mock<IUnitOfWork>();
            this.workspaces = Create.Storage.InMemoryPlainStorage<Workspace>();
            logger = new Mock<ILogger<DeleteWorkspaceSchemaJob>>();
            jobDataMap = new JobDataMap();
            jobDataMap["workspace"] = "test";
            jobDataMap["schema"] = "ws_test";

            Subject = new DeleteWorkspaceSchemaJob(this.unitOfWork.Object, workspaces, logger.Object);
        }

        [Test]
        public async Task should_not_delete_schema_if_there_is_working_workspace()
        {
            var workspace = Create.Entity.Workspace("test");
            this.workspaces.Store(workspace, workspace.Name);

            await Subject.Execute(Mock.Of<IJobExecutionContext>(j => j.MergedJobDataMap == jobDataMap));

            unitOfWork.Verify(u => u.Session, Times.Never);
        }

        [Test]
        public async Task should_not_delete_schema_if_schemaName_with_wrong_prefix()
        {
            var workspace = Create.Entity.Workspace("test");
            this.workspaces.Store(workspace, workspace.Name);

            jobDataMap["schema"] = "public";
            
            await Subject.Execute(Mock.Of<IJobExecutionContext>(j => j.MergedJobDataMap == jobDataMap));

            unitOfWork.Verify(u => u.Session, Times.Never);
        }
    }
}