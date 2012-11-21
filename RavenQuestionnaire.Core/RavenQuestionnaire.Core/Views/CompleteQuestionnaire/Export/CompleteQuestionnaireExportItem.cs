// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CompleteQuestionnaireExportItem.cs" company="The World Bank">
//   2012
// </copyright>
// <summary>
//   The complete questionnaire export item.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using System.Collections.Generic;
using Main.Core.Entities.SubEntities;

namespace RavenQuestionnaire.Core.Views.CompleteQuestionnaire.Export
{
    using System;
    using System.Linq;

    using Main.Core.Documents;
    using Main.Core.Entities.SubEntities.Complete;
    using Main.Core.View.CompleteQuestionnaire;

    /// <summary>
    /// The complete questionnaire export item.
    /// </summary>
    public class CompleteQuestionnaireExportItem
    {

        #region Constructors and Destructors

        /*/// <summary>
        /// Initializes a new instance of the <see cref="CompleteQuestionnaireExportItem"/> class.
        /// </summary>
        /// <param name="doc">
        /// The doc.
        /// </param>
        public CompleteQuestionnaireExportItem(CompleteQuestionnaireDocument doc)
        {
            this.CompleteQuestionnaireKey = doc.PublicKey;
            this.CompleteAnswers = doc.Find<ICompleteAnswer>(a => a.Selected).ToArray();
        }
        */
        /// <summary>
        /// Initializes a new instance of the <see cref="CompleteQuestionnaireExportItem"/> class.
        /// </summary>
        /// <param name="document">
        /// The document.
        /// </param>
        public CompleteQuestionnaireExportItem(ICompleteGroup document, IEnumerable<Guid> headerKey, Guid? parent)
        {
            var wholeQuestionnaire = document as CompleteQuestionnaireStoreDocument;
            Guid templateId = document.PublicKey;
            if (wholeQuestionnaire != null)
                templateId = wholeQuestionnaire.TemplateId;
            this.PublicKey = document.Propagated == Propagate.None ? document.PublicKey : document.PropagationPublicKey.Value;
            this.Values = new Dictionary<Guid, string>();

            foreach (Guid key in headerKey)
            {
                if (key == Guid.Empty)
                {
                    this.Values.Add(key, parent.ToString());
                }
                else if (templateId==key)
                {
                    this.Values.Add(key, this.PublicKey.ToString());
                }
                else
                {
                    var question = document.FirstOrDefault<ICompleteQuestion>(c => c.PublicKey == key);
                    this.Values.Add(key, question.GetAnswerString());
                }
            }
        }

        #endregion

        #region Public Properties

        public Dictionary<Guid,string> Values { get; set; }

   /*     /// <summary>
        /// Gets or sets the complete answers.
        /// </summary>
        public ICompleteAnswer[] CompleteAnswers { get; set; }*/

        /// <summary>
        /// Gets the complete questionnaire key.
        /// </summary>
        public Guid PublicKey { get; private set; }

        #endregion
    }
}