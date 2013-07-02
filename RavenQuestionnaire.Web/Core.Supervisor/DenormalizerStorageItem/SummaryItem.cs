// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SummaryItem.cs" company="">
//   
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

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
        }

        #region Public Properties

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

        public Guid? ResponsibleSupervisorId { get; set; }

        public string ResponsibleName { get; set; }

        public Guid QuestionnaireStatus { get; set; }

        #endregion
    }
}