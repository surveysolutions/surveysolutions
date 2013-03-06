
namespace WB.UI.Designer.Providers.CQRS.Roles.View
{
    public class RoleViewInputModel
    {
        public RoleViewInputModel(object providerUserKey)
        {
            ProviderUserKey = providerUserKey;
        }

        public RoleViewInputModel(string roleName)
        {
            RoleName = roleName;
        }

        /// <summary>
        /// Account Id
        /// </summary>
        public object ProviderUserKey { get; protected set; }
        /// <summary>
        /// Role name
        /// </summary>
        public string RoleName { get; protected set; }
    }
}
