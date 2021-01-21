using NHibernate.Mapping.ByCode;
using NHibernate.Mapping.ByCode.Conformist;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.DataCollection.Implementation.Repositories;

namespace WB.Core.BoundedContexts.Headquarters.Mappings
{
    public class AudioFileMap : ClassMapping<AudioFile>
    {
        public AudioFileMap()
        {
            this.Id(x => x.Id, idMap =>
            {
                idMap.Generator(Generators.Assigned);
                idMap.Column("Id");
            });

            this.Property(x => x.InterviewId);
            this.Property(x => x.FileName);

            this.Property(x => x.Data, dataMap =>
            {
                dataMap.Lazy(true);
            });

            this.Property(x => x.ContentType);
        }
    }
}
