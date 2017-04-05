﻿using System;
using System.Linq;
using System.Threading.Tasks;
using Main.Core.Entities.SubEntities;
using Moq;
using NUnit.Framework;
using WB.Core.BoundedContexts.Headquarters.OwinSecurity;
using WB.Tests.Abc;

namespace WB.Tests.Unit.BoundedContexts.Headquarters.Security
{
    [TestOf(typeof(HqUserManager))]
    internal class HqUserManagerTests
    {
        [Test]
        public async Task when_unarchive_interviewer_and_supervisor_is_archived_should_return_IdentityResult_Failed()
        {
            var supervisor = Create.Entity.HqUser(Guid.NewGuid(), role: UserRoles.Supervisor, isArchived: true);
            var interviewer = Create.Entity.HqUser(Guid.NewGuid(), supervisorId: supervisor.Id, isArchived: true);

            var userRepository = Mock.Of<IUserRepository>(x =>
                x.Users == new[] {supervisor, interviewer}.AsQueryable() &&
                x.FindByIdAsync(supervisor.Id) == Task.FromResult(supervisor));
            
            var userManager = Create.Storage.HqUserManager(userRepository);

            var archiveResult = await userManager.ArchiveUsersAsync(new[] {interviewer.Id}, false);

            Assert.IsFalse(archiveResult[0].Succeeded);
        }
    }
}
