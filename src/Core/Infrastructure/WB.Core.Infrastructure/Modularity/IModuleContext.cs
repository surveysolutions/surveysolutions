namespace WB.Core.Infrastructure.Modularity
{
    public interface IModuleContext
    {
        T Resolve<T>();
    }

    public interface IConstructorContext
    {
        T Inject<T>();
    }


}