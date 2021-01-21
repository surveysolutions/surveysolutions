using System.Collections.Generic;
using NHibernate.Mapping.ByCode;
using NHibernate.Mapping.ByCode.Conformist;
using WB.Core.BoundedContexts.Headquarters.Users.UserPreloading.Dto;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.InterviewEntities.Answers;
using WB.Infrastructure.Native.Storage.Postgre.NhExtensions;

namespace WB.Core.BoundedContexts.Headquarters.Users.UserPreloading.Mapping
{
    public class AssignmentToImportMap : ClassMapping<AssignmentToImport>
    {
        public AssignmentToImportMap()
        {
            this.Table("assignmenttoimport");
            this.Id(x => x.Id, Idmap => Idmap.Generator(Generators.Identity));
            this.Property(x => x.Interviewer);
            this.Property(x => x.Supervisor);
            this.Property(x => x.Quantity);
            this.Property(x => x.Error);
            this.Property(x => x.Verified);
            this.Property(x => x.Email);
            this.Property(x => x.Password);
            this.Property(x => x.WebMode);
            this.Property(x => x.IsAudioRecordingEnabled);
            this.Property(x => x.Headquarters);
            this.Property(x => x.Comments);

            this.Property(x => x.Answers, mapper =>
            {
                mapper.Type<PostgresJson<List<InterviewAnswer>>>();
            });
            this.Property(x => x.ProtectedVariables, m => m.Type<PostgresJson<List<string>>>());
        }
    }
}
