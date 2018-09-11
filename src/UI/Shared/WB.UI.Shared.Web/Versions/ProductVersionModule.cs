using System.Reflection;
using System.Threading.Tasks;
using WB.Core.GenericSubdomains.Portable.ServiceLocation;
using WB.Core.Infrastructure.Modularity;
using WB.Core.Infrastructure.Versions;

namespace WB.UI.Shared.Web.Versions
{
    public class ProductVersionModule : IModule
    {
        private readonly bool shouldStoreVersionToDb;
        private readonly ProductVersion productVersion;

        public ProductVersionModule(Assembly versionAssembly, bool shouldStoreVersionToDb = true)
        {
            this.shouldStoreVersionToDb = shouldStoreVersionToDb;
            this.productVersion = new ProductVersion(versionAssembly);
        }

        public void Load(IIocRegistry registry)
        {
            registry.BindToConstant<IProductVersion>(() => this.productVersion);
            registry.Bind<IProductVersionHistory, ProductVersionHistory>();
        }

        public Task Init(IServiceLocator serviceLocator, UnderConstructionInfo status)
        {
            if (shouldStoreVersionToDb)
                serviceLocator.GetInstance<IProductVersionHistory>().RegisterCurrentVersion();

            return Task.CompletedTask;
        }
    }
}
