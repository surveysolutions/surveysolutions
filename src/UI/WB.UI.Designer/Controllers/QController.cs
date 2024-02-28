using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Razor.TagHelpers;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Vite.Extensions.AspNetCore;

namespace WB.UI.Designer.Controllers;

[Route("/q")]
public class QController : Controller
{
    private readonly ITagHelperComponentManager tagHelperComponentManager;
    
    public QController(ITagHelperComponentManager tagHelperComponentManager,
        IWebHostEnvironment webHost,
        IOptions<ViteTagOptions> options,
        IMemoryCache memoryCache)
    {
        this.tagHelperComponentManager = tagHelperComponentManager;
        this.tagHelperComponentManager.Components.Add(new ViteTagHelperComponent(webHost, options, memoryCache));
    }
    
    [Route("{**catchAll}")]
    public ViewResult Index() => View("Vue");
}
