namespace WB.UI.Shared.Web.Authentication
{
    public class BasicCredentials
    {
        public BasicCredentials(string username, string password)
        {
            Username = username;
            Password = password;
        }

        public string Username { get; private set; }
        public string Password { get; private set; }
    }
}
