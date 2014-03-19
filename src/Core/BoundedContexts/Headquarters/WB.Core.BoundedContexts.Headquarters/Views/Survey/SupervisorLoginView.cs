using WB.Core.Infrastructure.ReadSide.Repository;

namespace WB.Core.BoundedContexts.Headquarters.Views.Survey
{
    public class SupervisorLoginView : IReadSideRepositoryEntity
    {
        public string Login { get; set; }

        public string PasswordHash { get; set; }
    }
}