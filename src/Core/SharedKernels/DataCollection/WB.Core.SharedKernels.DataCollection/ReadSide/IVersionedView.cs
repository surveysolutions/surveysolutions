using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WB.Core.Infrastructure.ReadSide;
using WB.Core.Infrastructure.ReadSide.Repository;

namespace WB.Core.SharedKernels.DataCollection.ReadSide
{
    public interface IVersionedView : IView
    {
        long Version { get; set; }
    }
}
