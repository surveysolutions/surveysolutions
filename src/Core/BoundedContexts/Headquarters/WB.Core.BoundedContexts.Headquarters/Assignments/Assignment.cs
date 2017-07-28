using System;
using System.Linq;
using System.Collections.Generic;
using WB.Core.BoundedContexts.Headquarters.Aggregates;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.InterviewEntities.Answers;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;

namespace WB.Core.BoundedContexts.Headquarters.Assignments
{
    public class Assignment
    {
        public Assignment()
        {
            this.CreatedAtUtc = DateTime.UtcNow;
            this.UpdatedAtUtc = DateTime.UtcNow;

            this.Answers = new List<InterviewAnswer>();
            this.IdentifyingData = new List<IdentifyingAnswer>();
            this.InterviewSummaries = new HashSet<InterviewSummary>();
        }

        public Assignment(QuestionnaireIdentity questionnaireId, Guid responsibleId, int? quantity) : this()
        {
            this.ResponsibleId = responsibleId;
            this.Quantity = quantity;
            this.QuestionnaireId = questionnaireId;

            Answers = new List<InterviewAnswer>();
            IdentifyingData = new List<IdentifyingAnswer>();
        }

        public virtual int Id { get; protected set; }

        public virtual Guid ResponsibleId { get; protected set; }

        public virtual ReadonlyUser Responsible { get; protected set; }

        public virtual int? Quantity { get; protected set; }
                
        public virtual bool Archived { get; protected set; }

        public virtual DateTime CreatedAtUtc { get; protected set; }

        public virtual DateTime UpdatedAtUtc { get; protected set; }

        public virtual QuestionnaireIdentity QuestionnaireId { get; set; }
        
        public virtual IList<IdentifyingAnswer> IdentifyingData { get; protected set; }

        public virtual IList<InterviewAnswer> Answers { get; protected set; }

        public virtual QuestionnaireLiteViewItem Questionnaire { get; set; }

        /// <summary>
        /// Will also include deleted interviews
        /// </summary>
        public virtual ISet<InterviewSummary> InterviewSummaries { get; protected set; }

        public virtual int InterviewsProvided =>
            InterviewSummaries.Count(i => i.Status == InterviewStatus.InterviewerAssigned ||
                                          i.Status == InterviewStatus.RejectedBySupervisor);

        public virtual int? InterviewsNeeded
        {
            get
            {
                return this.Quantity.HasValue
                    ? this.Quantity - this.InterviewSummaries.Count(x => !x.IsDeleted)
                    : null;
            }
        }

        public virtual bool IsCompleted => this.InterviewsNeeded <= 0;

        public virtual void Archive()
        {
            this.Archived = true;
            this.UpdatedAtUtc = DateTime.UtcNow;
        }

        public virtual void UpdateQuantity(int? quantity)
        {
            this.Quantity = quantity;
            this.UpdatedAtUtc = DateTime.UtcNow;
        }
        
        public virtual void Reassign(Guid responsibleId)
        {
            this.ResponsibleId = responsibleId;
            this.UpdatedAtUtc = DateTime.UtcNow;
        }

        public virtual void SetIdentifyingData(IList<IdentifyingAnswer> identifyingAnswers)
        {
            IdentifyingData = identifyingAnswers;
            this.UpdatedAtUtc = DateTime.UtcNow;
        }

        public virtual void SetAnswers(IList<InterviewAnswer> answers)
        {
            Answers = answers;
            this.UpdatedAtUtc = DateTime.UtcNow;
        }

        public virtual void Unarchive()
        {
            this.Archived = false;
            this.UpdatedAtUtc = DateTime.UtcNow;
        }
    }

    public class QuestionnaireLiteViewItem
    {
        public virtual string Title { get; set; }
        public virtual string Id { get; set; }
    }
}