using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Main.Core.View;

namespace WB.Core.Synchronization.Views
{
    public class IncomeSyncPackagesViewFactory : IViewFactory<IncomeSyncPackagesInputModel, IncomeSyncPackagesView>
    {
        private readonly IIncomePackagesRepository repository;

        public IncomeSyncPackagesViewFactory(IIncomePackagesRepository repository)
        {
            this.repository = repository;
        }

        public IncomeSyncPackagesView Load(IncomeSyncPackagesInputModel input)
        {
            return new IncomeSyncPackagesView(repository.GetIncomingItemsCount());
        }
    }
}
