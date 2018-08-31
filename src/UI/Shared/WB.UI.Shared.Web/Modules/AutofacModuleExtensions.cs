using Autofac;

namespace WB.UI.Shared.Web.Modules
{
    public static class AutofacModuleExtensions
    {
        public static Module AsWebAutofac<TModule>(this TModule module)
            where TModule : IWebModule
        {
            return new AutofacWebModuleAdapter(module);
        }
    }
}
