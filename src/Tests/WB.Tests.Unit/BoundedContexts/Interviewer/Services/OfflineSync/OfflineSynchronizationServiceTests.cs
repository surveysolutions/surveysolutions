using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using WB.Core.BoundedContexts.Interviewer.Implementation.Services;
using WB.Core.GenericSubdomains.Portable.Implementation;
using WB.Core.SharedKernels.DataCollection.WebApi;
using WB.Tests.Abc;

namespace WB.Tests.Unit.BoundedContexts.Interviewer.Services.OfflineSync
{
    public class OfflineSynchronizationServiceTests
    {
        [Test]
        public async Task should_always_return_offline_token_when_logged_in()
        {
            var service = Create.Service.OfflineSynchronizationService();

            var token = await service.LoginAsync(new LogonInfo(), new RestCredentials(), CancellationToken.None);

            Assert.That(token, Is.EqualTo("offline sync token"));
        }

        [Test]
        public async Task should_always_bypass_device_link_test()
        {
            var service = Create.Service.OfflineSynchronizationService();

            var hasDevice = await service.HasCurrentUserDeviceAsync();

            Assert.That(hasDevice, Is.True);
        }

        //[Test]
        //public void should_pass_device_build_number_to_can_synchronize_method()
        //{
            
        //}
    }
}
