using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace WB.UI.Headquarters.Services
{
    public interface IClientApkProvider
    {
        IActionResult GetApkAsHttpResponse(HttpRequest request, string appName, string responseFileName);
        IActionResult GetPatchFileAsHttpResponse(HttpRequest request, string fileName);
        int? GetApplicationBuildNumber(string appName);
        string GetApplicationVersionString(string appName);
        
        string ApkClientsFolder();
    }
}
