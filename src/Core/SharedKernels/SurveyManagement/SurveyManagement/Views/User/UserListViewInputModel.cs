using Main.Core.Entities.SubEntities;

namespace WB.Core.SharedKernels.SurveyManagement.Views.User
{
    /// <summary>
    /// The user list view input model.
    /// </summary>
    public class UserListViewInputModel : ListViewModelBase
    {
        public UserRoles Role = UserRoles.Undefined;
    }
}