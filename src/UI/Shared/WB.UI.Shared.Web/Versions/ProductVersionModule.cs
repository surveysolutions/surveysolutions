using System.Reflection;
using Ninject.Modules;
using WB.Core.Infrastructure.Versions;

namespace WB.UI.Shared.Web.Versions
{
    public class ProductVersionModule : NinjectModule
    {
        private readonly ProductVersion productVersion;

        public ProductVersionModule(Assembly versionAssembly)
        {
            this.productVersion = new ProductVersion(versionAssembly);
        }

        public override void Load()
        {
            this.Bind<IProductVersion>().ToConstant(this.productVersion);
            this.Bind<IProductVersionHistory>().To<ProductVersionHistory>();
        }
    }
}