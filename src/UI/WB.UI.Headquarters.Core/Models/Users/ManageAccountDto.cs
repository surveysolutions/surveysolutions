using System;

namespace WB.UI.Headquarters.Models.Users
{
    public class ManageAccountDto
    {
        public Guid UserId { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public string PersonName { get; set; }
        public string PhoneNumber { get; set; }
        public string Role { get; set; }
        public bool IsOwnProfile { get; set; }
        public bool IsLockedByHeadquarters { get; set; }
        public bool IsLockedBySupervisor { get; set; }
    }
}
