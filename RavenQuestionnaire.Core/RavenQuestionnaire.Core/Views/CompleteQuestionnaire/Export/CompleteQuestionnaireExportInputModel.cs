// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CompleteQuestionnaireExportInputModel.cs" company="The World Bank">
//   2012
// </copyright>
// <summary>
//   The complete questionnaire export input model.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace RavenQuestionnaire.Core.Views.CompleteQuestionnaire.Export
{
    using System;
    using System.Collections.Generic;

    using Main.Core.Entities;
    using Main.Core.Utility;

    /// <summary>
    /// The complete questionnaire export input model.
    /// </summary>
    public class CompleteQuestionnaireExportInputModel
    {

        #region Public Properties
        
        /// <summary>
        /// Gets or sets the questionnary id.
        /// </summary>
        public Guid QuestionnaryId { get; set; }

        public Guid? PropagatableGroupPublicKey { get; set; }

        #endregion
    }
}