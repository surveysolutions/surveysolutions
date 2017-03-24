using System;
using WB.Core.GenericSubdomains.Portable.CustomCollections;

namespace WB.Core.BoundedContexts.Headquarters.Services.Internal
{
    internal class RandomValuesSource : IRandomValuesSource
    {
        private readonly Random random = new Random();
        private readonly ConcurrentHashSet<int> generatedIds = new ConcurrentHashSet<int>();

        public int Next(int maxInterviewKeyValue)
        {
            var next = this.random.Next(maxInterviewKeyValue);
            while (this.generatedIds.Contains(next))
            {
                next = this.random.Next(maxInterviewKeyValue);
            }

            this.generatedIds.Add(next);
            return next;
        }
    }
}