using System.Threading.Tasks;

namespace WB.Core.Infrastructure.Modularity
{
    public interface IKernel
    {
        void Load<T>(params IModule<T>[] modules) where T : IIocRegistry;

        Task Init();
    }
}
