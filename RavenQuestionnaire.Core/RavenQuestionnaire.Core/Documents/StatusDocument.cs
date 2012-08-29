﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="StatusDocument.cs" company="The World Bank">
//   2012
// </copyright>
// <summary>
//   Describes the status in the system
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace RavenQuestionnaire.Core.Documents
{
    using System.Collections.Generic;

    using RavenQuestionnaire.Core.Entities.SubEntities;

    /// <summary>
    /// Describes the status in the system
    /// </summary>
    public class StatusDocument : AbstractDocument
    {
        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="StatusDocument"/> class.
        /// </summary>
        public StatusDocument()
        {
            this.Statuses = new List<StatusItem>();
        }

        #endregion

        #region Public Properties

        /// <summary>
        ///Containes ref to the correspondent Q.
        /// </summary>
        public string QuestionnaireId { get; set; }

        /// <summary>
        /// List of Statuses of Q.
        /// </summary>
        public List<StatusItem> Statuses { get; set; }

        #endregion
    }
}