using Main.Core.Entities.SubEntities;

namespace WB.Core.SharedKernels.SurveyManagement.Views.User
{
    /// <summary>
    /// The user list view input model.
    /// </summary>
    public class UserListViewInputModel : ListViewModelBase
    {
        #region Fields
       
        /// <summary>
        ///     Get users by role
        /// </summary>
        public UserRoles Role = UserRoles.Undefined;

        #endregion
    }
}