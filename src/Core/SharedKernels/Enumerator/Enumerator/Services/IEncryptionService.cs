namespace WB.Core.SharedKernels.Enumerator.Services
{
    public interface IEncryptionService
    {
        void GenerateKeys();
        string Encrypt(string textToEncrypt);
        string Decrypt(string textToDecrypt);
    }
}
