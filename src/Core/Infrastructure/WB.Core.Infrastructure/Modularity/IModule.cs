namespace WB.Core.Infrastructure.Modularity
{
    public interface IModule
    {
        void Load(IIocRegistry registry);
    }
}