using System;

namespace WB.Core.BoundedContexts.Headquarters.Implementation.ReadSide
{
    public class ReadSideDenormalizerStatistic
    {
        public ReadSideDenormalizerStatistic(string denormalizerName, TimeSpan timeSpent, int percent)
        {
            this.DenormalizerName = denormalizerName;
            this.TimeSpent = timeSpent;
            this.Percent = percent;
        }

        public string DenormalizerName { get; private set; }
        public TimeSpan TimeSpent { get; private set; }
        public int Percent { get; private set; }
    }
}
