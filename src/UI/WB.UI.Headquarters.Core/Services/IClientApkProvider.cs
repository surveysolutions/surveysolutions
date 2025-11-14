using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace WB.UI.Headquarters.Services
{
    public interface IClientApkProvider
    {
        Task<IActionResult> GetApkAsHttpResponse(HttpRequest request, string appName, string responseFileName);
        Task<IActionResult> GetPatchFileAsHttpResponse(HttpRequest request, string fileName);
        Task<int?> GetApplicationBuildNumber(string appName);
        Task<string> GetApplicationVersionString(string appName);
        Task<long?> GetApplicationSize(string appName);
        
        string ApkClientsFolder();
    }
}
