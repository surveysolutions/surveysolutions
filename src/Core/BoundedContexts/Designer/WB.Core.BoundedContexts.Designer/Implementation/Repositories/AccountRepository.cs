using System;
using Microsoft.Practices.ServiceLocation;
using WB.Core.BoundedContexts.Designer.Aggregates;
using WB.Core.BoundedContexts.Designer.Views.Account;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.Aggregates;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;

namespace WB.Core.BoundedContexts.Designer.Implementation.Repositories
{
    internal class AccountRepository : IPlainAggregateRootRepository<AccountAR>
    {
        private readonly IReadSideRepositoryWriter<AccountDocument> storage;
        private readonly IServiceLocator serviceLocator;

        public AccountRepository(
            IServiceLocator serviceLocator,
            IReadSideRepositoryWriter<AccountDocument> storage)
        {
            this.serviceLocator = serviceLocator;
            this.storage = storage;
        }

        public AccountAR Get(Guid aggregateId)
        {
            var document = this.storage.GetById(aggregateId.FormatGuid());

            if (document == null)
                return null;

            var account = this.serviceLocator.GetInstance<AccountAR>();
            account.Document = document;
            return account;
        }

        public void Save(AccountAR account)
        {
            var document = account.Document;
            this.storage.Store(document, account.Id.FormatGuid());
        }
    }
}