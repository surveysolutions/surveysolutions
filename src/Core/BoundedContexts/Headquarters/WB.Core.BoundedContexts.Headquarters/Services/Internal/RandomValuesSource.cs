using System;

namespace WB.Core.BoundedContexts.Headquarters.Services.Internal
{
    internal class RandomValuesSource : IRandomValuesSource
    {
        private readonly Random random = new Random();
        private readonly object lockObject = new object();

        public int Next(int maxInterviewKeyValue)
        {
            lock (lockObject)
            {
                return random.Next(maxInterviewKeyValue);
            }
        }
    }
}
