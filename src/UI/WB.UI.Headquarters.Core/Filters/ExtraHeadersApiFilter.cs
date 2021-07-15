using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Net.Http.Headers;
using System.Linq;

namespace WB.UI.Headquarters.Filters
{
    public class ExtraHeadersApiFilter : IActionFilter
    {
        private string[] EsriFontsLocations = { "https://js.arcgis.com" };
        private string[] EsriImagesLocations = { "http://js.arcgis.com", "https://server.arcgisonline.com", "https://services.arcgisonline.com" };

        private string[] GoogleMapsFontsLocations = { "https://fonts.gstatic.com" };
        private string[] GoogleMapsImagesLocations  = { "https://maps.googleapis.com", "https://maps.gstatic.com", "https://google.com"};

        public void OnActionExecuting(ActionExecutingContext context)
        {         
            if (context.HttpContext.Request.Path.StartsWithSegments("/graphql"))
            { 
            }
            else if (context.HttpContext.Request.Path.StartsWithSegments("/api"))
            {
                context.HttpContext.Response.GetTypedHeaders().CacheControl =
                    new CacheControlHeaderValue()
                    {
                        NoStore = true,
                        NoCache = true,
                    };
            }
            else
            {
                AddOrUpdateHeader(context,"X-Xss-Protection", "1");
                AddOrUpdateHeader(context,"X-Content-Type-Options", "nosniff");                
                AddOrUpdateHeader(context,"X-Frame-Options", "SAMEORIGIN");
                AddOrUpdateHeader(context,"Content-Security-Policy", BuildCSPValue(context));
            }
        }

        public void OnActionExecuted(ActionExecutedContext context)
        {
        }

        private string BuildCSPValue(ActionExecutingContext context)
        { 
            string fontsSources = ""; 
            string imageSources = "";

            if (context.ActionDescriptor is ControllerActionDescriptor controllerContext)                 
            { 
                var permissionsAttribute = controllerContext.MethodInfo.GetCustomAttributes(true).OfType<ExtraHeaderPermissionsAttribute>().FirstOrDefault();
                if(permissionsAttribute != null)
                {
                    foreach (var item in permissionsAttribute.PermissionTypes)
                    {
                        if(item == HeaderPermissionType.Google)
                        { 
                            fontsSources = fontsSources + " " + string.Join(' ', GoogleMapsFontsLocations);
                            imageSources = imageSources + " " + string.Join(' ', GoogleMapsImagesLocations);
                        }

                        if(item == HeaderPermissionType.Esri)
                        { 
                            fontsSources = fontsSources + " " + string.Join(' ', EsriFontsLocations);
                            imageSources = imageSources + " " + string.Join(' ', EsriImagesLocations);
                        }
                    } 
                }
            }

            return $"font-src 'self' data:{fontsSources}; img-src 'self' data: blob:{imageSources}; default-src * data: 'self' 'unsafe-inline' 'unsafe-eval' blob:";
        }

        private void AddOrUpdateHeader(ActionExecutingContext context, string key, string value)
        { 
            if(context.HttpContext.Response.Headers.ContainsKey(key))
                    context.HttpContext.Response.Headers.Remove(key);
                context.HttpContext.Response.Headers.Add(key, value);
        }
    }
}
