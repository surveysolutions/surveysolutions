using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WB.Core.Infrastructure.Implementation.ReadSide
{
    public class ReadSideDenormalizerStatistic
    {
        public ReadSideDenormalizerStatistic(string denormalizerName, TimeSpan timeSpent)
        {
            DenormalizerName = denormalizerName;
            TimeSpent = timeSpent;
        }

        public string DenormalizerName { get; private set; }
        public TimeSpan TimeSpent { get; private set; }
    }
}
