namespace Core.Supervisor.Views.User
{
    using Main.Core.Entities.SubEntities;

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