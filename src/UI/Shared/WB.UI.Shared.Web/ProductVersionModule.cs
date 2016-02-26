using System.Diagnostics;
using System.Reflection;
using Ninject.Modules;
using WB.Core.Infrastructure.Versions;

namespace WB.UI.Shared.Web
{
    public class ProductVersionModule : NinjectModule
    {
        private class ProductVersion : IProductVersion
        {
            private readonly Assembly assembly;

            public ProductVersion(Assembly assembly)
            {
                this.assembly = assembly;
            }

            public override string ToString() => FileVersionInfo.GetVersionInfo(assembly.Location).ProductVersion;
        }

        private readonly ProductVersion productVersion;

        public ProductVersionModule(Assembly versionAssembly)
        {
            this.productVersion = new ProductVersion(versionAssembly);
        }

        public override void Load()
        {
            this.Bind<IProductVersion>().ToConstant(productVersion);
        }
    }
}