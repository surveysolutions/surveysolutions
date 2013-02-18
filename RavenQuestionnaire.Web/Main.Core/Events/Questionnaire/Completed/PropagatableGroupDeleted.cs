﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PropagatableGroupDeleted.cs" company="The World Bank">
//   2012
// </copyright>
// <summary>
//   The propagatable group deleted.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Main.Core.Events.Questionnaire.Completed
{
    using System;

    using Ncqrs.Eventing.Storage;

    /// <summary>
    /// The propagate group deleted.
    /// </summary>
    [Serializable]
    [EventName("RavenQuestionnaire.Core:Events:PropagatableGroupDeleted")]
    public class PropagatableGroupDeleted
    {
        #region Public Properties

        /*/// <summary>
        /// Gets or sets the completed questionnaire id.
        /// </summary>
        [Obsolete]
        public Guid CompletedQuestionnaireId { get; set; }*/

        /// <summary>
        /// Gets or sets the propagation key.
        /// </summary>
        public Guid PropagationKey { get; set; }

        /// <summary>
        /// Gets or sets the public key.
        /// </summary>
        public Guid PublicKey { get; set; }
        
        /// <summary>
        /// Gets or sets the parent key.
        /// </summary>
        public Guid ParentKey { get; set; }

        /// <summary>
        /// Gets or sets the parent propagation key.
        /// </summary>
        public Guid? ParentPropagationKey { get; set; }
        
        /*/// <summary>
        /// Gets or sets the question propagation key.
        /// </summary>
        public Guid? QuestionPropagationKey { get; set; }*/

        #endregion
    }
}