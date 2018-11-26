using Autofac;

namespace WB.UI.Shared.Web.Modules
{
    public static class AutofacModuleExtensions
    {
        public static Module AsWebAutofac(this IWebModule module)
        {
            return new AutofacWebModuleAdapter(module);
        }
    }
}
