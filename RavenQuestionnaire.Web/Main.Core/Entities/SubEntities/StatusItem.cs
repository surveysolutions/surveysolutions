namespace Main.Core.Entities.SubEntities
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// The status item.
    /// </summary>
    public class StatusItem
    {
        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="StatusItem"/> class.
        /// </summary>
        public StatusItem()
        {
            this.PublicKey = Guid.NewGuid();
            this.IsVisible = true;
            this.StatusRoles = new Dictionary<string, List<SurveyStatus>>();
            this.FlowRules = new Dictionary<Guid, FlowRule>();
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// List of flow rules is used for status changing.
        /// </summary>
        public Dictionary<Guid, FlowRule> FlowRules { get; set; }

        /// <summary>
        /// Flag displays status is used for stuck item in the commonon flow.
        /// </summary>
        public bool IsDefaultStuck { get; set; }

        /// <summary>
        /// Is used for defining of the status for initially created CQ.
        /// </summary>
        public bool IsInitial { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether is visible.
        /// </summary>
        public bool IsVisible { get; set; }

        /// <summary>
        /// Gets or sets the public key.
        /// </summary>
        public Guid PublicKey { get; set; }

        /// <summary>
        /// Holds restriction by role.
        /// </summary>
        public Dictionary<string, List<SurveyStatus>> StatusRoles { get; set; }

        /// <summary>
        /// Gets or sets the title.
        /// </summary>
        public string Title { get; set; }

        #endregion
    }
}