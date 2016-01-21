namespace WB.Infrastructure.Security
{
    public interface ICypherManager
    {
        bool EncryptionEnforced();
        string GetPassword();

        void SetEncryptionEnforcement(bool value);
        void RegeneratePassword();
    }
}
