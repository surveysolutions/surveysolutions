using System;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;
using Main.Core.Entities.SubEntities;
using Moq;
using NUnit.Framework;
using WB.Core.BoundedContexts.Headquarters.Implementation.Services;
using WB.Core.BoundedContexts.Headquarters.Users;
using WB.Core.BoundedContexts.Headquarters.Views.User;
using WB.Tests.Abc;

namespace WB.Tests.Unit.BoundedContexts.Headquarters;

[TestFixture]
public class UserToDeviceServiceTests
{
    [Test]
    public async Task when_try_link_device_without_relink_flag()
    {
        var user = Mock.Of<HqUser>(u =>
            u.IsInRole(UserRoles.Interviewer) == true
            && u.Profile.DeviceId == "oldDeviceId"
            && u.Profile.DeviceRegistrationDate == DateTime.UtcNow
            && u.Profile.IsRelinkAllowed() == false);
        var userRepository = Mock.Of<IUserRepository>(r => 
            r.FindByIdAsync(Id.g1, It.IsAny<CancellationToken>()) == Task.FromResult(user));
        var service = CreateService(userRepository);

        var exception = Assert.CatchAsync(async () => await service.LinkDeviceToUserAsync(Id.g1, "deviceId"));

        Assert.That(exception, Is.Not.Null);
        Assert.That(exception.GetType(), Is.EqualTo(typeof(InvalidOperationException)));
        Assert.That(exception.Message, Is.EqualTo("You must have approval from supervisor or headquarters to relink device"));
    }

    private IUserToDeviceService CreateService(IUserRepository userRepository) => new UserToDeviceService(userRepository);
}