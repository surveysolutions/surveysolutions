namespace WB.Core.BoundedContexts.Headquarters.Implementation.Services.HealthCheck.Checks
{
    public interface IAtomicHealthCheck<T> where T:class 
    {
        T Check();
    }
}