using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Razor.TagHelpers;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Vite.Extensions.AspNetCore;

namespace WB.UI.Designer.Controllers
{
    [Authorize]
    public class ClassificationsController : Controller
    {
        public ClassificationsController(ITagHelperComponentManager tagHelperComponentManager,
            IWebHostEnvironment webHost,
            IOptions<ViteTagOptions> options,
            IMemoryCache memoryCache)
        {
            tagHelperComponentManager.Components.Add(new ViteTagHelperComponent(webHost, options, memoryCache));
        }
        
        public IActionResult Index() => this.View("Vue");
    }
}
