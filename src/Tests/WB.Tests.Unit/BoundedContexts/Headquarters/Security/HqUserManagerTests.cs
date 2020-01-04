namespace WB.Tests.Unit.BoundedContexts.Headquarters.Security
{
    // TODO: KP-13523 Fix HqUserManager related tests
    //[TestOf(typeof(HqUserManager))]
    //internal class HqUserManagerTests
    //{
    //    [Test]
    //    public async Task when_unarchive_interviewer_and_supervisor_is_archived_should_return_IdentityResult_Failed()
    //    {
    //        var supervisor = Create.Entity.HqUser(Guid.NewGuid(), role: UserRoles.Supervisor, isArchived: true);
    //        var interviewer = Create.Entity.HqUser(Guid.NewGuid(), supervisorId: supervisor.Id, isArchived: true);

    //        var userRepository = Mock.Of<IUserRepository>(x =>
    //            x.Users == new[] {supervisor, interviewer}.AsQueryable() &&
    //            x.FindByIdAsync(supervisor.Id) == Task.FromResult(supervisor));

    //        var userManager = Create.Storage.HqUserManager(userRepository);

    //        var archiveResult = await userManager.UnarchiveUsersAsync(new[] {interviewer.Id});

    //        Assert.IsFalse(archiveResult[0].Succeeded);
    //    }
    //}
}
