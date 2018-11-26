namespace WB.Core.Infrastructure.Modularity.Autofac
{
    public static class ModuleExtensions
    {
        public static global::Autofac.Core.IModule AsAutofac(this Core.Infrastructure.Modularity.IModule ourModule)
        {
            return new AutofacModuleAdapter(ourModule);
        }
    }
}
