using WB.Core.SharedKernels.SurveySolutions;

namespace WB.Core.Infrastructure.ReadSide.Repository.Accessors
{
    public class ReadSideRepositoryReaderVersionedWrapper<TEntity>
        where TEntity : class, IReadSideRepositoryEntity
    {
        private readonly IReadSideRepositoryReader<TEntity> reader;

        public ReadSideRepositoryReaderVersionedWrapper(IReadSideRepositoryReader<TEntity> reader)
        {
            this.reader = reader;
        }

        public TEntity Get(string id, long version)
        {
            string versionedId = GetVersionedId(id, version);

            return this.reader.GetById(versionedId);
        }

        private static string GetVersionedId(string id, long version)
        {
            return string.Format("{0}${1}", id, version);
        }
    }
}