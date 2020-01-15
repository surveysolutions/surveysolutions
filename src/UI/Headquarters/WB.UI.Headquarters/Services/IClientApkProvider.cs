using System.Net.Http;

namespace WB.UI.Headquarters.Services
{
    public interface IClientApkProvider
    {
        HttpResponseMessage GetApkAsHttpResponse(HttpRequestMessage request, string appName, string responseFileName);
        HttpResponseMessage GetPatchFileAsHttpResponse(HttpRequestMessage request, string fileName);
        int? GetApplicationBuildNumber(string appName);
        string GetApplicationVersionString(string appName);
        string ApkClientsFolder();
    }
}
