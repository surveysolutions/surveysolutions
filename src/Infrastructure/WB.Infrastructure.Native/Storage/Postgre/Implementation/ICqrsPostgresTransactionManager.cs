using WB.Core.Infrastructure.PlainStorage;
using WB.Core.Infrastructure.Transactions;

namespace WB.Infrastructure.Native.Storage.Postgre.Implementation
{
    internal interface ICqrsPostgresTransactionManager : ITransactionManager, ISessionProvider { }

    internal interface IPlainPostgresTransactionManager : IPlainTransactionManager, ISessionProvider { }
}