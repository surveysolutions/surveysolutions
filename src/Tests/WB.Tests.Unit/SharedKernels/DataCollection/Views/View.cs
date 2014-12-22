using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WB.Core.SharedKernels.DataCollection.ReadSide;

namespace WB.Tests.Unit.SharedKernels.DataCollection.Views
{
    public class View : IVersionedView
    {
        public long Version { get; set; }
    }
}
