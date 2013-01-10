// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SurveyScreen.cs" company="The World Bank">
//   The World Bank
// </copyright>
// <summary>
//   The survey screen.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Core.Supervisor.Documents
{
    using System;

    using Main.Core.Documents;
    using Main.Core.Utility;

    /// <summary>
    /// The survey screen.
    /// </summary>
    public class SurveyScreen
    {
        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="SurveyScreen"/> class.
        /// </summary>
        /// <param name="doc">
        /// The doc.
        /// </param>
        /// <param name="node">
        /// The group.
        /// </param>
        public SurveyScreen(CompleteQuestionnaireDocument doc, NodeWithLevel node)
        {
            if (node == null)
            {
                return;
            }
            var stringKey = node.Group.PublicKey.ToString()
                            +
                            (node.Group.PropagationPublicKey.HasValue
                                 ? node.Group.PropagationPublicKey.Value
                                 : Guid.Empty).ToString();

            this.Key = Helper.StringToGuid(stringKey);
        }

        #endregion

        public Guid Key { get; private set; }
    }
}