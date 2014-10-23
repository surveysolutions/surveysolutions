using System.Collections.Generic;
using WB.Core.Infrastructure.ReadSide;

namespace WB.Core.Infrastructure.Implementation
{
    public class ReadSideRepositoryWriterRegistry : IReadSideRepositoryWriterRegistry
    {
        private readonly List<IReadSideRepositoryWriter> writers = new List<IReadSideRepositoryWriter>();

        public void Register(IReadSideRepositoryWriter writer)
        {
            this.writers.Add(writer);
        }

        public IEnumerable<IReadSideRepositoryWriter> GetAll()
        {
            return this.writers;
        }
    }
}