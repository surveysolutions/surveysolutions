namespace WB.Core.SharedKernels.DataCollection.Services
{
    public interface IEncryptionService
    {
        void GenerateKeys();
        string Encrypt(string textToEncrypt);
        string Decrypt(string textToDecrypt);
        byte[] Encrypt(byte[] value);
        byte[] Decrypt(byte[] value);
    }
}
