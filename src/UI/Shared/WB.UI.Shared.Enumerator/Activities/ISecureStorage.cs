namespace WB.UI.Shared.Enumerator.Activities
{
    public interface ISecureStorage
    {
        void Store(string key, byte[] dataBytes);
        byte[] Retrieve(string key);
        void Delete(string key);
        bool Contains(string key);
    }
}
