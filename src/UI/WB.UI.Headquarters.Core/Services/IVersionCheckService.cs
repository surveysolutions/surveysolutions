namespace WB.UI.Headquarters.Services
{
    public interface IVersionCheckService
    {
        bool DoesNewVersionExist();
        string GetNewVersionString();
    }
}