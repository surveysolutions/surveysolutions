using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Main.Core.Entities.SubEntities;
using Moq;
using NUnit.Framework;
using WB.Core.BoundedContexts.Headquarters.Users;
using WB.Tests.Abc;

namespace WB.Tests.Unit.BoundedContexts.Headquarters.Security
{
    [TestOf(typeof(UserArchiveService))]
    internal class UserArchiveServiceTests
    {
        [Test]
        public void when_unarchive_interviewer_and_supervisor_is_archived_should_throw_UserArchiveException()
        {
            var supervisor = Create.Entity.HqUser(Guid.NewGuid(), role: UserRoles.Supervisor, isArchived: true);
            var interviewer = Create.Entity.HqUser(Guid.NewGuid(), supervisorId: supervisor.Id, isArchived: true);

            var userRepository = Mock.Of<IUserRepository>(x =>
                x.Users == new[] {supervisor, interviewer}.AsQueryable() &&
                x.FindByIdAsync(supervisor.Id, CancellationToken.None) == Task.FromResult(supervisor));

            var userManager = Create.Service.UserArchiveService(userRepository);

            Assert.ThrowsAsync<UserArchiveException>(() => 
                userManager.UnarchiveUsersAsync(new[] {interviewer.Id}));
        }
    }
}
