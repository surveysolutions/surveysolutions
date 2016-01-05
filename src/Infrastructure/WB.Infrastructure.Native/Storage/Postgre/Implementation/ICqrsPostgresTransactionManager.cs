using WB.Core.Infrastructure.Transactions;

namespace WB.Infrastructure.Native.Storage.Postgre.Implementation
{
    internal interface ICqrsPostgresTransactionManager : ITransactionManager, ISessionProvider { }
}