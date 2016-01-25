namespace WB.Infrastructure.Security
{
    public interface IExportSettings
    {
        bool EncryptionEnforced();
        string GetPassword();

        void SetEncryptionEnforcement(bool value);
        void RegeneratePassword();
    }
}
