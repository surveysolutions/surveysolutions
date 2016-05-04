namespace WB.Core.Infrastructure.Modularity
{
    public interface IIocRegistry
    {
        void Bind<TInterface, TImplementation>() where TImplementation : TInterface;
        void BindAsSingleton<TInterface, TImplementation>() where TImplementation : TInterface;
        void BindAsSingletonWithConstructorArgument<TInterface, TImplementation>(string argumentName, object argumentValue) where TImplementation : TInterface;
        void BindToRegisteredInterface<TInterface, TRegisteredInterface>() where TRegisteredInterface : TInterface;
    }
}