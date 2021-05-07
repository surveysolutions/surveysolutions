using NHibernate.Mapping.ByCode;
using NHibernate.Mapping.ByCode.Conformist;
using WB.Core.BoundedContexts.Headquarters.Assignments;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;

namespace WB.Core.BoundedContexts.Headquarters.Invitations
{
    public class InvitationMap : ClassMapping<Invitation>
    {
        public InvitationMap()
        {
            Id(x => x.Id, mapper => mapper.Generator(Generators.Identity));
            DynamicUpdate(true);
            Property(x => x.AssignmentId);
            Property(x => x.InterviewId);
            Property(x => x.Token);
            Property(x => x.ResumePassword);
            Property(x => x.SentOnUtc);
            Property(x => x.LastRejectedInterviewEmailId, c => c.Column("last_rejected_interview_email_id"));
            Property(x => x.LastRejectedStatusPosition, c => c.Column("last_rejected_status_position"));
            Property(x => x.InvitationEmailId);
            Property(x => x.LastReminderSentOnUtc);
            Property(x => x.LastReminderEmailId);
            Property(x => x.NumberOfRemindersSent);

            ManyToOne(x => x.Assignment, mto =>
            {
                mto.Column("AssignmentId");
                mto.PropertyRef(nameof(Assignment.Id));
                mto.Cascade(Cascade.None);
                mto.Update(false);
                mto.Insert(false);
            });

            ManyToOne(x => x.Interview, mto =>
            {
                mto.Column("InterviewId");
                mto.PropertyRef(nameof(InterviewSummary.SummaryId));
                mto.Cascade(Cascade.None);
                mto.Update(false);
                mto.Insert(false);
            });
        }
    }
}
