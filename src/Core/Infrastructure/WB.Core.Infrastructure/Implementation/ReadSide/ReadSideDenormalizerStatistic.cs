using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WB.Core.Infrastructure.Implementation.ReadSide
{
    public class ReadSideDenormalizerStatistic
    {
        public ReadSideDenormalizerStatistic(string denormalizerName, TimeSpan timeSpent, int percent)
        {
            DenormalizerName = denormalizerName;
            TimeSpent = timeSpent;
            Percent = percent;
        }

        public string DenormalizerName { get; private set; }
        public TimeSpan TimeSpent { get; private set; }
        public int Percent { get; private set; }
    }
}
