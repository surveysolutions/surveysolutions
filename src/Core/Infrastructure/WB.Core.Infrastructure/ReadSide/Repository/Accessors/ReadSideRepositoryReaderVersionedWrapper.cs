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
            string versionedId = ReadSideExtensions.AsCompositeKey(id, version);

            return this.reader.GetById(versionedId);
        }
    }
}