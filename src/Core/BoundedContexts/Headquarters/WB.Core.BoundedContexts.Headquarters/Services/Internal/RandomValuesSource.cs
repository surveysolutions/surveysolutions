using System;

namespace WB.Core.BoundedContexts.Headquarters.Services.Internal
{
    internal class RandomValuesSource : IRandomValuesSource
    {
        private readonly Random random = new Random();

        public int Next(int maxInterviewKeyValue)
        {
            return this.random.Next(maxInterviewKeyValue);
        }
    }
}