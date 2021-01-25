using WB.Core.Infrastructure.Domain;

namespace WB.Tests.Abc
{
    public static class Extensions
    {
        public static NoScopeInScopeExecutor<T> AsNoScopeExecutor<T>(this T service)
        {
            return new(service);
        }
    }
}
