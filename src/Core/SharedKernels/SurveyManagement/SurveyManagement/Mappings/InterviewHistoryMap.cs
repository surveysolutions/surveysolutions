using NHibernate.Mapping.ByCode;
using NHibernate.Mapping.ByCode.Conformist;
using WB.Core.SharedKernels.SurveyManagement.Implementation.Services.DataExport;
using WB.Core.SharedKernels.SurveyManagement.Views.DataExport;

namespace WB.Core.SharedKernels.SurveyManagement.Mappings
{
    public class InterviewHistoryMap : ClassMapping<InterviewHistory>
    {
        public InterviewHistoryMap()
        {
            Table("InterviewHistory");

            Id(x => x.InterviewId, idMap =>
            {
                idMap.Generator(Generators.Assigned);
                idMap.Column("Id");
            });
            Property(x => x.QuestionnaireId);
            Property(x => x.QuestionnaireVersion);

            Set(x => x.InterviewActions, set =>
            {
                set.Key(key => key.Column("InterviewId"));
                set.Lazy(CollectionLazy.NoLazy);
                set.Cascade(Cascade.All | Cascade.DeleteOrphans);
            },
                relation => relation.OneToMany());
        }
    }

    public class InterviewActionMap : ClassMapping<InterviewAction>
    {
        public InterviewActionMap()
        {
            Id(x => x.Id, Idmap => Idmap.Generator(Generators.HighLow));
            Property(x => x.Action);
            Property(x => x.Originator);
            Property(x => x.Role);
            Property(x => x.Timestamp);
            ManyToOne(x => x.History, mto =>
            {
                mto.Index("InterviewHistorys_InterviewActions");
                mto.Cascade(Cascade.None);
            });
        }
    }
}