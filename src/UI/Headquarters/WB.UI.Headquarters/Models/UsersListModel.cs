using System.Collections.Generic;

namespace WB.UI.Headquarters.Models
{
    public class UsersListModel
    {
        public UsersListModel()
        {
            Users = new List<UserListDto>();
        }

        public List<UserListDto> Users { get; set; }
        public string HighlightedUser { get; set; }
    }

    public class UserListDto
    {
        public string Id { get; set; }
        public string Login { get; set; }
        public string Role { get; set; }
    }
}