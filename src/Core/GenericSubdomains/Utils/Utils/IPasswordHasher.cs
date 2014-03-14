namespace WB.Core.GenericSubdomains.Utils
{
    public interface IPasswordHasher
    {
        string Hash(string password);
    }
}
