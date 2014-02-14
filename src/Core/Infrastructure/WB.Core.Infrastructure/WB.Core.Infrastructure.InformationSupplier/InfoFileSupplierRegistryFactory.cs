using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WB.Core.Infrastructure.InformationSupplier.Implementation.InfoFileSupplierRegistry;

namespace WB.Core.Infrastructure.InformationSupplier
{
    public class InfoFileSupplierRegistryFactory
    {
        public IInfoFileSupplierRegistry CreateInfoFileSupplierRegistry()
        {
            return new DefaultInfoFileSupplierRegistry();
        }
    }
}
