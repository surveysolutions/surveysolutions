namespace WB.Core.Infrastructure.Modularity
{
    public interface IIocRegistry
    {
        void Bind<TInterface, TImplementation>() where TImplementation : TInterface;
        void Singleton<TInterface, TImplementation>() where TImplementation : TInterface;
    }
}