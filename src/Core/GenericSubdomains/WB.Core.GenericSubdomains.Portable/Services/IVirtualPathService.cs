namespace WB.UI.Shared.Web.Services
{
    public interface IVirtualPathService
    {
        string GetAbsolutePath(string relativePath);
        string GetRelatedToRootPath(string relativeUrl);
    }
}
