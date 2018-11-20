using System.Reflection;
using System.Threading.Tasks;
using NHibernate;
using WB.Core.GenericSubdomains.Portable.ServiceLocation;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.Modularity;
using WB.Core.Infrastructure.Versions;
using WB.Infrastructure.Native.Storage.Postgre;

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
            {
                using (var unitOfWork = new UnitOfWork(serviceLocator.GetInstance<ISessionFactory>(), serviceLocator.GetInstance<ILogger>()))
                {
                    serviceLocator.GetInstance<IProductVersionHistory>().RegisterCurrentVersion();
                    unitOfWork.AcceptChanges();
                }
            }

            return Task.CompletedTask;
        }
    }
}
