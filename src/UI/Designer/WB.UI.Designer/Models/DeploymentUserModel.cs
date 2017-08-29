using System;

namespace WB.UI.Designer.Models
{
    public class DeploymentUserModel
    {
        public Guid Id { get; set; }
        public string[] Roles { get; set; }
        public string Login { get; set; }
        public string Email { get; set; }
    }
}