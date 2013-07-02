// -----------------------------------------------------------------------
// <copyright file="SummaryInputModel.cs" company="">
// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------

namespace Core.Supervisor.Views.Summary
{
    using System;

    public class SummaryInputModel : ListViewModelBase
    {
        public SummaryInputModel(Guid viewerId, ViewerStatus viewerStatus)
        {
            this.ViewerId = viewerId;
            this.ViewerStatus = viewerStatus;
        }

        public ViewerStatus ViewerStatus { get; set; }

        /// <summary>
        /// Gets or sets ViewerId.
        /// </summary>
        public Guid ViewerId { get; set; }

        /// <summary>
        /// Gets or sets TemplateId.
        /// </summary>
        public Guid? TemplateId { get; set; }
    }
}
