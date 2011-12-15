namespace RavenQuestionnaire.Core.Views.Status.SubView
{
    public class RolePermission
    {
        public string RoleName { set; get; }
        public bool Permit { set; get; }

        public  RolePermission(string role, bool permit)
        {
            RoleName = role;
            Permit = permit;
        }

        public  RolePermission()
        {
            RoleName = string.Empty;
            Permit = false;
        }
    }
}
