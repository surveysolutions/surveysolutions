namespace RavenQuestionnaire.Core.Views.User
{
    public class UserViewInputModel
    {
        public UserViewInputModel(string id)
        {
            UserId = id;
        }

        public UserViewInputModel(string username, string password)
        {
            this.UserName = username;
            this.Password = password;
        }
        public string UserId { get; private set; }
        public string UserName { get; private set; }
        public string Password { get; private set; }
    }
}
