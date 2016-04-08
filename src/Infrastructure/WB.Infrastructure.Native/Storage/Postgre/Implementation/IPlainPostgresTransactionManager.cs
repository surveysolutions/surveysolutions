using WB.Core.Infrastructure.PlainStorage;

namespace WB.Infrastructure.Native.Storage.Postgre.Implementation
{
    internal interface IPlainPostgresTransactionManager : IPlainTransactionManager, ISessionProvider { }
}