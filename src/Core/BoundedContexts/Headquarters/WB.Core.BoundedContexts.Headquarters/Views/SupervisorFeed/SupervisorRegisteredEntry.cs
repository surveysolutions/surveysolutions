using WB.Core.Infrastructure.ReadSide.Repository;

namespace WB.Core.BoundedContexts.Headquarters.Views.SupervisorFeed
{
    public class SupervisorRegisteredEntry : IReadSideRepositoryEntity
    {
        public string Login { get; set; }

        public string PasswordHash { get; set; }
    }
}