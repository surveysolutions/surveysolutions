namespace WB.Core.SharedKernels.DataCollection.Services
{
    public interface ISecureStorage
    {
        void Store(string key, byte[] dataBytes);
        byte[] Retrieve(string key);
        void Delete(string key);
        bool Contains(string key);
    }
}
