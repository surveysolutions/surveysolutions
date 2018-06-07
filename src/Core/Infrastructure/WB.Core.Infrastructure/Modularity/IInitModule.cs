using System.Threading.Tasks;
using WB.Core.GenericSubdomains.Portable.ServiceLocation;

namespace WB.Core.Infrastructure.Modularity
{
    public interface IInitModule
    {
        Task Init(IServiceLocator serviceLocator, UnderConstructionInfo status);
    }
}
