using System;

namespace WB.UI.Designer.Models
{
    public class PortalUserModel
    {
        public Guid Id { get; set; }
        public string[] Roles { get; set; }
        public string Login { get; set; }
        public string Email { get; set; }
        public string FullName { get; set; }
    }
}
