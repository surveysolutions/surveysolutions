using NHibernate.Mapping.ByCode;
using NHibernate.Mapping.ByCode.Conformist;
using WB.Core.BoundedContexts.Supervisor.Interviews.Implementation.Views;

namespace WB.Core.BoundedContexts.Supervisor.Mappings
{
    public class ReadyToSendToHeadquartersInterviewMap : ClassMapping<ReadyToSendToHeadquartersInterview>
    {
        public ReadyToSendToHeadquartersInterviewMap()
        {
            Id(x => x.Id, idMap => idMap.Generator(Generators.Assigned));

            Property(x => x.InterviewId);
        }
    }
}