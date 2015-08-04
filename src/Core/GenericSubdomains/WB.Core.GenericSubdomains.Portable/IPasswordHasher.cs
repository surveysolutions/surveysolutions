namespace WB.Core.GenericSubdomains.Portable
{
    public interface IPasswordHasher
    {
        string Hash(string password);
    }
}
