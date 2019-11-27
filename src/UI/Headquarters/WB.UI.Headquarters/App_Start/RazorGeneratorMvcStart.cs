using System.Diagnostics;
using System.Web;
using System.Web.Mvc;
using System.Web.WebPages;
using RazorGenerator.Mvc;

namespace WB.UI.Headquarters
{
    public static class RazorGeneratorMvcStart
    {
    
        public static void Start()
        {
#if !DEBUG
            var engine = new PrecompiledMvcEngine(typeof(RazorGeneratorMvcStart).Assembly)
            {
                UsePhysicalViewsIfNewer = false //HttpContext.Current.Request.IsLocal
            };

            ViewEngines.Engines.Insert(0, engine);
            
            // StartPage lookups are done by WebPages. 
            VirtualPathFactoryManager.RegisterVirtualPathFactory(engine);
#endif
        }
    }
}
