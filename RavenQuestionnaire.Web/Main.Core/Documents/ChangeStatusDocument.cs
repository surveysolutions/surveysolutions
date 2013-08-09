namespace Main.Core.Documents
{
    using System;

    using Main.Core.Entities.SubEntities;

    /// <summary>
    /// The change status document.
    /// </summary>
    public class ChangeStatusDocument
    {
        #region Public Properties

        /// <summary>
        /// Gets or sets ChangeDate.
        /// </summary>
        public DateTime ChangeDate { get; set; }

        /// <summary>
        /// Gets or sets Responsible.
        /// </summary>
        public UserLight Responsible { get; set; }

        /// <summary>
        /// Gets or sets Status.
        /// </summary>
        public SurveyStatus Status { get; set; }

        #endregion
    }
}