using System.Collections.Generic;

namespace WB.Core.Infrastructure.Raven.Implementation.ReadSide
{
    internal class RavenReadSideRepositoryWriterRegistry : IRavenReadSideRepositoryWriterRegistry
    {
        private readonly List<IRavenReadSideRepositoryWriter> writers = new List<IRavenReadSideRepositoryWriter>();

        public void Register(IRavenReadSideRepositoryWriter writer)
        {
            this.writers.Add(writer);
        }

        public IEnumerable<IRavenReadSideRepositoryWriter> GetAll()
        {
            return this.writers;
        }
    }
}