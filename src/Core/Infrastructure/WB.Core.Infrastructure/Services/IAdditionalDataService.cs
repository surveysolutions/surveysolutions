using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WB.Core.Infrastructure.Services
{
    public interface IAdditionalDataService<TEntity>
    {
        void CheckAdditionalRepository(string id);
    }
}
