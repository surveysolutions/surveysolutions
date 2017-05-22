using System;
using System.Collections.Generic;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;

namespace WB.Core.BoundedContexts.Headquarters.Assignments
{
    public class Assignment
    {
        public Assignment()
        {
            this.IdentifyingData = new List<IdentifyingAnswer>();
            this.CreatedAtUtc = DateTime.UtcNow;
            this.UpdatedAtUtc = DateTime.UtcNow;
        }

        public virtual int Id { get; protected set; }

        public virtual Guid ResponsibleId { get; protected set; }

        public virtual int? Capacity { get; protected set; }

        public virtual bool Archived { get; protected set; }

        public virtual DateTime CreatedAtUtc { get; protected set; }

        public virtual DateTime UpdatedAtUtc { get; protected set; }

        public virtual QuestionnaireIdentity QuestionnaireId { get; set; }

        public virtual IList<IdentifyingAnswer> IdentifyingData { get; protected set; }

        public virtual void Archive()
        {
            this.Archived = true;
            this.UpdatedAtUtc = DateTime.UtcNow;
        }

        public virtual void UpdateCapacity(int? capacity)
        {
            this.Capacity = capacity;
            this.UpdatedAtUtc = DateTime.UtcNow;
        }

        public virtual void Reassign(Guid responsibleId)
        {
            this.ResponsibleId = responsibleId;
            this.UpdatedAtUtc = DateTime.UtcNow;
        }
    }
}