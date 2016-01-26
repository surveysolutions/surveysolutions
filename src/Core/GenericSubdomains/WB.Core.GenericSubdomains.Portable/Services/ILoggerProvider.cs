namespace WB.Core.GenericSubdomains.Portable.Services
{
    public interface ILoggerProvider
    {
        ILogger GetFor<T>();
    }
}