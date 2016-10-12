using System;
using WB.Core.BoundedContexts.Designer.Aggregates;
using WB.Core.BoundedContexts.Designer.Views.Account;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.Aggregates;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;

namespace WB.Core.BoundedContexts.Designer.Implementation.Repositories
{
    internal class AccountRepository : IPlainAggregateRootRepository<Account>
    {
        private readonly IPlainStorageAccessor<Account> storage;

        public AccountRepository(IPlainStorageAccessor<Account> storage)
        {
            this.storage = storage;
        }

        public Account Get(Guid aggregateId) => this.storage.GetById(aggregateId.FormatGuid());

        public void Save(Account account) => this.storage.Store(account, account.Id.FormatGuid());
    }
}