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

        public CompleteQuestionnaireExportInputModel(IEnumerable<Guid> questionnairiesForImport, Guid templateId, Guid? propagatableGroupPublicKey)
            : this(questionnairiesForImport, templateId)
        {
            PropagatableGroupPublicKey = propagatableGroupPublicKey;
        }
        public CompleteQuestionnaireExportInputModel(IEnumerable<Guid> questionnairiesForImport, Guid templateId)
        {
            QuestionnairiesForImport = questionnairiesForImport;
            TemplateId = templateId;
        }

        public Guid TemplateId { get; private set; }
        public IEnumerable<Guid> QuestionnairiesForImport { get;private set; }
        public Guid? PropagatableGroupPublicKey { get; private set; }

        #endregion
    }
}