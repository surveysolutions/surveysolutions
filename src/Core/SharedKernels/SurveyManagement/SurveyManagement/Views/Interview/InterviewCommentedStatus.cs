using System;
using System.Collections.Generic;
using NHibernate.Mapping.ByCode;
using NHibernate.Mapping.ByCode.Conformist;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Core.SharedKernels.SurveyManagement.Views.DataExport;
using WB.Core.SharedKernels.SurveySolutions.Documents;

namespace WB.Core.SharedKernels.SurveyManagement.Views.Interview
{
    public class InterviewStatuses : IView
    {
        public InterviewStatuses()
        {
            this.InterviewCommentedStatuses = new HashSet<InterviewCommentedStatus>();
        }

        public virtual string InterviewId { get; set; }
        public virtual Guid QuestionnaireId { get; set; }
        public virtual long QuestionnaireVersion { get; set; }
        public virtual ISet<InterviewCommentedStatus> InterviewCommentedStatuses { get; set; }
    }

    public class InterviewCommentedStatus
    {
        public InterviewCommentedStatus()
        {
        }

        public InterviewCommentedStatus(
            Guid statusChangeOriginatorId, 
            Guid? supervisorId,
            Guid? interviewerId, 
            InterviewStatus status, 
            DateTime timestamp, 
            string comment, 
            string statusChangeOriginatorName)
        {
            StatusChangeOriginatorId = statusChangeOriginatorId;
            SupervisorId = supervisorId;
            InterviewerId = interviewerId;
            Status = status;
            Timestamp = timestamp;
            Comment = comment;
            StatusChangeOriginatorName = statusChangeOriginatorName;
        }
        public virtual int Id { get; set; }
        public virtual Guid? SupervisorId { get; set; }
        public virtual Guid? InterviewerId { get; set; }
        public virtual Guid StatusChangeOriginatorId { get; set; }
        public virtual string StatusChangeOriginatorName { get; set; }

        public virtual InterviewStatus Status { get; set; }
        public virtual DateTime Timestamp { get; set; }

        public virtual string Comment { get; set; }

        public virtual InterviewStatuses InterviewStatuses { get; set; }
    }


    public class InterviewStatusesMap : ClassMapping<InterviewStatuses>
    {
        public InterviewStatusesMap()
        {
            Table("InterviewStatuses");

            Id(x => x.InterviewId, idMap =>
            {
                idMap.Generator(Generators.Assigned);
                idMap.Column("Id");
            });
            Property(x => x.QuestionnaireId);
            Property(x => x.QuestionnaireVersion);

            Set(x => x.InterviewCommentedStatuses, set =>
            {
                set.Key(key => key.Column("InterviewId"));
                set.Lazy(CollectionLazy.NoLazy);
                set.Cascade(Cascade.All | Cascade.DeleteOrphans);
            },
                relation => relation.OneToMany());
        }
    }

    public class InterviewCommentedStatusMap : ClassMapping<InterviewCommentedStatus>
    {
        public InterviewCommentedStatusMap()
        {
            Id(x => x.Id, Idmap => Idmap.Generator(Generators.HighLow));
            Property(x => x.SupervisorId);
            Property(x => x.InterviewerId);
            Property(x => x.StatusChangeOriginatorId);
            Property(x => x.Timestamp);
            Property(x => x.StatusChangeOriginatorName);
            Property(x => x.Status);
            Property(x => x.Comment);
            ManyToOne(x => x.InterviewStatuses, mto =>
            {
                mto.Index("InterviewStatuseses_InterviewCommentedStatuses");
                mto.Cascade(Cascade.None);
            });
        }
    }
}