using System.Threading.Tasks;
using NUnit.Framework;
using WB.Core.BoundedContexts.Supervisor;
using WB.Core.BoundedContexts.Supervisor.Services.Implementation;
using WB.Core.SharedKernels.Enumerator.OfflineSync.Messages;
using WB.Core.SharedKernels.Enumerator.Utils;
using WB.Tests.Abc;

namespace WB.Tests.Unit.BoundedContexts.Supervisor.Services
{
    [TestOf(typeof(SupervisorQuestionnaireHandler))]
    public class SupervisorQuestionnaireHandlerTests
    {
        [Test]
        public async Task CanSynchronize_should_check_assemblyFileVersion_for_compatibility()
        {
            var handler = Create.Service.SupervisorQuestionnaireHandler();

            var expectedVersion = ReflectionUtils.GetAssemblyVersion(typeof(SupervisorBoundedContextAssemblyIndicator));
            var response = await handler.Handle(new CanSynchronizeRequest(expectedVersion.Revision));

            Assert.That(response, Has.Property(nameof(response.CanSyncronize)).True);
        }

        [Test]
        public async Task CanSynchronize_should_check_assemblyFileVersion_for_incompatibility()
        {
            var handler = Create.Service.SupervisorQuestionnaireHandler();

            var response = await handler.Handle(new CanSynchronizeRequest(1));

            Assert.That(response, Has.Property(nameof(response.CanSyncronize)).False);
        }
    }
}
