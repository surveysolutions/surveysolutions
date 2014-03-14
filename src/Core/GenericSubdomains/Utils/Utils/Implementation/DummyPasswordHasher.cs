namespace WB.Core.GenericSubdomains.Utils.Implementation
{
    public class DummyPasswordHasher :  IPasswordHasher
    {
        public string Hash(string password)
        {
            var passwordBytes = System.Text.Encoding.UTF8.GetBytes(password);
            return System.Convert.ToBase64String(passwordBytes);
        }
    }
}