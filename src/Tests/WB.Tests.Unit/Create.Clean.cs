using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WB.Tests.Unit.TestFactories;

namespace WB.Tests.Unit
{
    internal partial class Create
    {
        public static readonly CommandFactory Command = new CommandFactory();

        public static readonly EventFactory Event = new EventFactory();
    }
}
