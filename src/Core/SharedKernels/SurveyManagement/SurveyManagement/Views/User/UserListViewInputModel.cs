using Main.Core.Entities.SubEntities;

namespace WB.Core.SharedKernels.SurveyManagement.Views.User
{
    public class UserListViewInputModel : ListViewModelBase
    {
        public UserRoles Role = UserRoles.Undefined;

        public string SearchBy { get; set; }

        public bool Archived { get; set; }
    }
}