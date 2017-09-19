using Main.Core.Entities.SubEntities;
using NHibernate.Mapping.ByCode;
using NHibernate.Mapping.ByCode.Conformist;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Core.SharedKernels.DataCollection.Events.Interview.Dtos;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.InterviewEntities.Answers;
using WB.Core.SharedKernels.DataCollection.Views.Interview;
using WB.Core.SharedKernels.Questionnaire.Documents;
using WB.Infrastructure.Native.Storage.Postgre.NhExtensions;

namespace WB.Core.BoundedContexts.Headquarters.Mappings
{

    public class InterviewEntityMap : ClassMapping<InterviewEntity>
    {
        public InterviewEntityMap()
        {
            this.Table("interviews");

            this.Id(x => x.Id, Idmap => Idmap.Generator(Generators.Identity));
            this.Property(x => x.InterviewId, pm => pm.Column(cm => cm.Index("interviews_interviewId")));

            this.Component(x => x.Identity, cmp =>
            {
                cmp.Property(y => y.Id, ptp => ptp.Column("entityid"));
                cmp.Property(x => x.RosterVector, ptp =>
                {
                    ptp.Type<PostgresRosterVector>();
                    ptp.Column(clm => clm.SqlType("int[]"));
                });
            });
            this.Property(x => x.HasFlag);
            this.Property(x => x.IsEnabled);
            this.Property(x => x.IsReadonly);
            this.Property(x => x.EntityType);
            this.Property(x => x.AnswerType);
            this.Property(x => x.InvalidValidations, ptp =>
            {
                ptp.Type<PostgresSqlArrayType<int>>();
                ptp.Column(clm => clm.SqlType("int[]"));
            });
            this.Property(x => x.AsArea, ptp => ptp.Type<PostgresEntityJson<Area>>());
            this.Property(x => x.AsIntArray, ptp =>
            {
                ptp.Type<PostgresSqlArrayType<int>>();
                ptp.Column(clm => clm.SqlType("int[]"));
            });
            this.Property(x => x.AsAudio, ptp => ptp.Type<PostgresEntityJson<AudioAnswer>>());
            this.Property(x => x.AsBool);
            this.Property(x => x.AsDateTime);
            this.Property(x => x.AsDouble);
            this.Property(x => x.AsGps, ptp => ptp.Type<PostgresEntityJson<GeoPosition>>());
            this.Property(x => x.AsInt);
            this.Property(x => x.AsIntMatrix, ptp => ptp.Type<PostgresEntityJson<int[][]>>());
            this.Property(x => x.AsList, ptp => ptp.Type<PostgresEntityJson<InterviewTextListAnswer[]>>());
            this.Property(x => x.AsLong);
            this.Property(x => x.AsString);
            this.Property(x => x.AsYesNo, ptp => ptp.Type<PostgresEntityJson<AnsweredYesNoOption[]>>());
        }
    }
}