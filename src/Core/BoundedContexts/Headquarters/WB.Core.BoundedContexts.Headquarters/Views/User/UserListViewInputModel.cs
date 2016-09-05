using Main.Core.Entities.SubEntities;

namespace WB.Core.BoundedContexts.Headquarters.Views.User
{
    public class UserListViewInputModel : ListViewModelBase
    {
        public UserRoles Role = 0;

        public string SearchBy { get; set; }

        public bool Archived { get; set; }
    }
}