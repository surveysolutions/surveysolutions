using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web.Mvc;
using Newtonsoft.Json;
using WB.UI.Headquarters.Code;

namespace WB.UI.Headquarters.Filters
{
    public class NewVersionAvailableFilter : ActionFilterAttribute
    {
        private static VersionCheckingInfo AvailableVersion = null;
        private static DateTime? LastLoadedAt = null;
        private static int SecondsForCacheIsValid = 60*60;

        private static DateTime? ErrorOccuredAt = null;
        private static int DelayOnErrorInSeconds = 3 * 60;


        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            if (ApplicationSettings.NewVersionCheckEnabled)
            {
                if (!string.IsNullOrEmpty(ApplicationSettings.NewVersionCheckUrl))
                {
                    filterContext.Controller.ViewBag.LastAvailableApplicationVersion = AvailableVersion;

                    if (LastLoadedAt == null || (DateTime.Now - LastLoadedAt)?.Seconds > SecondsForCacheIsValid )
                    {
                        if ((ErrorOccuredAt == null) || (ErrorOccuredAt != null && (DateTime.Now - ErrorOccuredAt)?.Seconds > DelayOnErrorInSeconds))
                        {
                            Task.Run(async () =>
                            {
                                await UpdateVersion(ApplicationSettings.NewVersionCheckUrl);
                            });
                        }
                    }
                }
            }

            base.OnActionExecuting(filterContext);
        }

        private async Task UpdateVersion(string uri)
        {
            try
            {
                var versionInfo = await DoRequestJsonAsync<VersionCheckingInfo>(ApplicationSettings.NewVersionCheckUrl);
                AvailableVersion = versionInfo;
                LastLoadedAt = DateTime.Now;
                ErrorOccuredAt = null;

            }
            catch (Exception)
            {
                ErrorOccuredAt = DateTime.Now;
            }
        }

        private async Task<T> DoRequestJsonAsync<T>(string uri)
        {
            using (var httpClient = new HttpClient())
            {
                httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
               
                try
                {
                    var  jsonData = await httpClient.GetStringAsync(uri);
                    return !string.IsNullOrEmpty(jsonData) ? JsonConvert.DeserializeObject<T>(jsonData) : default(T);
                }
                catch (Exception)
                {
                    return default(T);
                }
                
            }
        }
    }
}