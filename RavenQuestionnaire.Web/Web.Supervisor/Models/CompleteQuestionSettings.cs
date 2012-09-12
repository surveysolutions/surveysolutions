// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CompleteQuestionSettings.cs" company="World bank">
//   2012
// </copyright>
// <summary>
//   Defines the CompleteQuestionSettings type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Web.Supervisor.Models
{
    using System;

    /// <summary>
    /// Define complete question settings
    /// </summary>
    public class CompleteQuestionSettings
    {
        /// <summary>
        /// Gets or sets QuestionnaireId.
        /// </summary>
        public string QuestionnaireId { get; set; }

        /// <summary>
        /// Gets or sets ParentGroupPublicKey.
        /// </summary>
        public Guid ParentGroupPublicKey { get; set; }

        /// <summary>
        /// Gets or sets PropogationPublicKey.
        /// </summary>
        public Guid? PropogationPublicKey { get; set; }
    }
}