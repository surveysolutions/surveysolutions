using Main.Core.Entities.SubEntities;

namespace WB.Core.BoundedContexts.Headquarters.Team.Models
{
    public class UserListViewInputModel : ListViewModelBase
    {
        public UserRoles Role = UserRoles.Undefined;
    }
}
