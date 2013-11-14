namespace WB.UI.QuestionnaireTester.Authentication
{
    public class UserInfo
    {
        public UserInfo(string userName, string password)
        {
            this.UserName = userName;
            this.Password = password;
        }

        public string UserName { get; private set; }
        public string Password { get; private set; }
    }
}