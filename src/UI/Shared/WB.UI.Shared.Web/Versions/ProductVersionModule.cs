using System.Reflection;
using WB.Core.Infrastructure.Modularity;
using WB.Core.Infrastructure.Versions;

namespace WB.UI.Shared.Web.Versions
{
    public class ProductVersionModule : IModule
    {
        private readonly ProductVersion productVersion;

        public ProductVersionModule(Assembly versionAssembly)
        {
            this.productVersion = new ProductVersion(versionAssembly);
        }

        public void Load(IIocRegistry registry)
        {
            registry.BindToConstant<IProductVersion>(() => this.productVersion);
            registry.Bind<IProductVersionHistory, ProductVersionHistory>();
        }
    }
}