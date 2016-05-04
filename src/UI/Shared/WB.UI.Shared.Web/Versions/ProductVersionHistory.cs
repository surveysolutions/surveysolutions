using System;
using System.Collections.Generic;
using System.Linq;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.Infrastructure.Transactions;
using WB.Core.Infrastructure.Versions;

namespace WB.UI.Shared.Web.Versions
{
    internal class ProductVersionHistory : IProductVersionHistory
    {
        private readonly IProductVersion productVersion;
        private readonly IPlainStorageAccessor<ProductVersionChange> historyStorage;
        private readonly IPlainTransactionManagerProvider plainTransactionManagerProvider;

        public ProductVersionHistory(IProductVersion productVersion, IPlainStorageAccessor<ProductVersionChange> historyStorage, IPlainTransactionManagerProvider plainTransactionManagerProvider)
        {
            this.productVersion = productVersion;
            this.historyStorage = historyStorage;
            this.plainTransactionManagerProvider = plainTransactionManagerProvider;
        }

        private string CurrentVersion => this.productVersion.ToString();

        public void RegisterCurrentVersion()
        {
            if (this.GetLastRegisteredVersion() == this.CurrentVersion)
                return;

            var versionChange = new ProductVersionChange(this.CurrentVersion, DateTime.UtcNow);

            this.plainTransactionManagerProvider.GetPlainTransactionManager().ExecuteInPlainTransaction(() =>
            {
                this.historyStorage.Store(versionChange, versionChange.UpdateTimeUtc);
            });
        }

        private string GetLastRegisteredVersion()
            => this.plainTransactionManagerProvider.GetPlainTransactionManager().ExecuteInPlainTransaction(()
                => this.historyStorage.Query(_
                    => _.OrderByDescending(change => change.UpdateTimeUtc)
                        .FirstOrDefault()?
                        .ProductVersion));

        public IEnumerable<ProductVersionChange> GetHistory()
            => this.plainTransactionManagerProvider.GetPlainTransactionManager().ExecuteInPlainTransaction(()
                => this.historyStorage.Query(_
                    => _.OrderByDescending(change => change.UpdateTimeUtc)
                        .ToList()));
    }
}