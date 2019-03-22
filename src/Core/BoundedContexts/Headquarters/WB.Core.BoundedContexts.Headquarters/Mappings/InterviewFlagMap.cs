using NHibernate.Mapping.ByCode.Conformist;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;

namespace WB.Core.BoundedContexts.Headquarters.Mappings
{
    public class InterviewFlagMap : ClassMapping<InterviewFlag>
    {
        public InterviewFlagMap()
        {
            ComposedId(m =>
            {
                m.Property(x => x.InterviewId);
                m.Property(x => x.QuestionIdentity);
            });
        }
    }
}
