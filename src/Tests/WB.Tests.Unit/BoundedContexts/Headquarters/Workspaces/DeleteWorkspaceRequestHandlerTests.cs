using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using Quartz;
using WB.Core.BoundedContexts.Headquarters.DataExport;
using WB.Core.BoundedContexts.Headquarters.Factories;
using WB.Core.BoundedContexts.Headquarters.Repositories;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.BoundedContexts.Headquarters.Workspaces;
using WB.Core.BoundedContexts.Headquarters.Workspaces.Jobs;
using WB.Core.Infrastructure.Domain;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Infrastructure.Native.Workspaces;
using WB.Tests.Abc;
using WB.UI.Headquarters.Code.Workspaces;

namespace WB.Tests.Unit.BoundedContexts.Headquarters.Workspaces
{
    [TestFixture(TestOf = typeof(DeleteWorkspaceRequestHandler))]
    public class DeleteWorkspaceRequestHandlerTests
    {
        private DeleteWorkspaceRequestHandler Subject;

        private IPlainStorageAccessor<Workspace> workspaces;

        private Mock<IQuestionnaireBrowseViewFactory> questionnaireViewFactory;
        private Mock<IMapStorageService> mapStorage;
        private Mock<IExportServiceApi> exportApi;
        private Mock<IScheduler> scheduler;
        private Mock<IWorkspacesService> workspaceService;
        private Mock<IWorkspacesCache> cache;

        private readonly Workspace Workspace = Create.Entity.Workspace("test");

        [SetUp]
        public void Setup()
        {
            questionnaireViewFactory = new Mock<IQuestionnaireBrowseViewFactory>();
            mapStorage = new Mock<IMapStorageService>();
            exportApi = new Mock<IExportServiceApi>();
            scheduler = new Mock<IScheduler>();
            workspaceService = new Mock<IWorkspacesService>();

            this.workspaces = Create.Storage.InMemoryPlainStorage<Workspace>();

            cache = new Mock<IWorkspacesCache>();
            cache.Setup(c => c.AllWorkspaces()).Returns(() =>
                this.workspaces.Query(_ => _.Select(w => w.AsContext())
                .ToList()));

            var executor = new NoScopeInScopeExecutor<IMapStorageService, IWorkspacesService>(
                mapStorage.Object, workspaceService.Object);

            Subject = new DeleteWorkspaceRequestHandler(
                 cache.Object,
                 questionnaireViewFactory.Object.AsNoScopeExecutor(),
                 executor,
                 exportApi.Object.AsNoScopeExecutor(),
                 scheduler.Object, Mock.Of<ISystemLog>());

            StoreWorkspaces(Workspace);
        }
        [Test]
        public async Task should_not_allow_to_delete_primary_workspace()
        {
            StoreWorkspaces(Create.Entity.Workspace(WorkspaceConstants.DefaultWorkspaceName));

            //act
            var response = await Subject.Handle(new DeleteWorkspaceRequest(WorkspaceConstants.DefaultWorkspaceName));

            Assert.That(response, Has.Property(nameof(DeleteWorkspaceResponse.Success)).False);
        }

        [Test]
        public async Task should_not_allow_to_delete_if_there_is_imported_questionnaire()
        {
            this.questionnaireViewFactory.Setup(q => q.GetAllQuestionnaireIdentities())
                .Returns(new List<QuestionnaireIdentity> { Create.Entity.QuestionnaireIdentity() });

            //act
            var response = await Subject.Handle(new DeleteWorkspaceRequest(Workspace.Name));

            Assert.That(response, Has.Property(nameof(DeleteWorkspaceResponse.Success)).False);
        }

        [Test]
        public async Task should_call_for_export_data_cleanup()
        {
            // act
            var response = await Subject.Handle(new DeleteWorkspaceRequest(Workspace.Name));

            exportApi.Verify(a => a.DeleteTenant(), Times.Once);

            Assert.That(response, Has.Property(nameof(DeleteWorkspaceResponse.Success)).True);
        }

        [Test]
        public async Task should_delete_maps()
        {
            // act
            var response = await Subject.Handle(new DeleteWorkspaceRequest(Workspace.Name));

            mapStorage.Verify(m => m.DeleteAllMaps(), Times.Once);

            Assert.That(response, Has.Property(nameof(DeleteWorkspaceResponse.Success)).True);
        }

        [Test]
        public async Task should_invalidate_cache()
        {
            // act
            var response = await Subject.Handle(new DeleteWorkspaceRequest(Workspace.Name));

            cache.Verify(c => c.InvalidateCache(), Times.Once);

            Assert.That(response, Has.Property(nameof(DeleteWorkspaceResponse.Success)).True);
        }

        [Test]
        public async Task should_schedule_workspace_schema_deletion()
        {
            // act
            var response = await Subject.Handle(new DeleteWorkspaceRequest(Workspace.Name));

            scheduler.Verify(s => s.ScheduleJob(
                It.Is<ITrigger>(d =>
                        d.JobDataMap.Contains(new KeyValuePair<string, object>("workspace", Workspace.Name))
                    && d.JobDataMap.Contains(
                                             new KeyValuePair<string, object>("schema", Workspace.AsContext().SchemaName))),
                    It.IsAny<CancellationToken>()), Times.Once);

            Assert.That(response, Has.Property(nameof(DeleteWorkspaceResponse.Success)).True);
        }

        private void StoreWorkspaces(params Workspace[] workspaceItems)
        {
            foreach (var workspace in workspaceItems)
            {
                this.workspaces.Store(workspace, workspace.Name);
            }
        }
    }
}
