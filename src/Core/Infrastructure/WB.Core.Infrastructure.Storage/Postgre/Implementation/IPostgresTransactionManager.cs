using System;
using WB.Core.Infrastructure.Transactions;

namespace WB.Core.Infrastructure.Storage.Postgre.Implementation
{
    internal interface IPostgresTransactionManager : ITransactionManager, ISessionProvider { }
}