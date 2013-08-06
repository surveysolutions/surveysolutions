using System.Collections.Generic;

namespace Core.Supervisor.DenormalizerStorageItem
{
    using System;

    using WB.Core.Infrastructure.ReadSide;

    public class SummaryItem : IView
    {
        public SummaryItem()
        {
            this.UnassignedCount = 0;
            this.ApprovedCount = 0;
            this.CompletedCount = 0;
            this.CompletedWithErrorsCount = 0;
            this.InitialCount = 0;
            this.RedoCount = 0;
            this.TotalCount = 0;
            this.DeletedQuestionnaries = new HashSet<Guid>();
        }

        public int UnassignedCount { get; set; }

        public int ApprovedCount { get; set; }

        public int CompletedCount { get; set; }

        public int CompletedWithErrorsCount { get; set; }

        public int InitialCount { get; set; }

        public int RedoCount { get; set; }

        public int TotalCount { get; set; }

        public Guid TemplateId { get; set; }

        public string TemplateName { get; set; }

        public Guid ResponsibleId { get; set; }

        public Guid CurrentStatusId { get; set; }

        public HashSet<Guid> DeletedQuestionnaries { get; set; }

        /// <summary>
        /// Name of resposible, which is a supervisor or an interviewer.
        /// </summary>
        public string ResponsibleName { get; set; }

        public Guid? ResponsibleSupervisorId { get; set; }

        /// <summary>
        /// Name of supervisor (which is a team lead), needed for team-based reports.
        /// </summary>
        public string ResponsibleSupervisorName { get; set; }
    }
}