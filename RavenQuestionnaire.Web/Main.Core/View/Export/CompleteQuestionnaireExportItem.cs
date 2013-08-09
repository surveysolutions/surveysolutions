using System;
using System.Collections.Generic;
using Main.Core.Entities.SubEntities;
using Main.Core.Entities.SubEntities.Complete;

namespace Main.Core.View.Export
{
    /// <summary>
    /// The complete questionnaire export item.
    /// </summary>
    public class CompleteQuestionnaireExportItem
    {

        #region Constructors and Destructors

      
        /// <summary>
        /// Initializes a new instance of the <see cref="CompleteQuestionnaireExportItem"/> class.
        /// </summary>
        /// <param name="document">
        /// The document.
        /// </param>
        public CompleteQuestionnaireExportItem(ICompleteGroup document, IEnumerable<Guid> headerKey, Guid? parent)
        {
            this.PublicKey = document.Propagated == Propagate.None ? document.PublicKey : document.PropagationPublicKey.Value;
            this.Parent = parent;
            this.Values = new ValueCollection();

            foreach (Guid key in headerKey)
            {
                var question = document.FirstOrDefault<ICompleteQuestion>(c => c.PublicKey == key);
                this.Values.Add(key, question);
            }
        }

        #endregion

        #region Public Properties

        public ValueCollection Values { get; set; }

  

        /// <summary>
        /// Gets the complete questionnaire key.
        /// </summary>
        public Guid PublicKey { get; private set; }
        public Guid? Parent { get; private set; }
        #endregion
    }
}