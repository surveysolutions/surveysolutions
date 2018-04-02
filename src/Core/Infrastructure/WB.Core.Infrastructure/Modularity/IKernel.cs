namespace WB.Core.Infrastructure.Modularity
{
    public interface IKernel
    {
        void Load(params IModule[] modules);

        void Init();
    }
}