// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ICompleteAnswerFactory.cs" company="The World Bank">
//   2012
// </copyright>
// <summary>
//   The CompleteAnswerFactory interface.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace RavenQuestionnaire.Core.AbstractFactories
{
    using RavenQuestionnaire.Core.Documents;
    using RavenQuestionnaire.Core.Entities.SubEntities;
    using RavenQuestionnaire.Core.Entities.SubEntities.Complete;
    using RavenQuestionnaire.Core.Views.Answer;

    /// <summary>
    /// The CompleteAnswerFactory interface.
    /// </summary>
    public interface ICompleteAnswerFactory
    {
        #region Public Methods and Operators

        /// <summary>
        /// The convert to complete answer.
        /// </summary>
        /// <param name="answer">
        /// The answer.
        /// </param>
        /// <returns>
        /// The RavenQuestionnaire.Core.Entities.SubEntities.Complete.ICompleteAnswer.
        /// </returns>
        ICompleteAnswer ConvertToCompleteAnswer(IAnswer answer);

        /// <summary>
        /// The create group.
        /// </summary>
        /// <param name="doc">
        /// The doc.
        /// </param>
        /// <param name="answer">
        /// The answer.
        /// </param>
        /// <returns>
        /// The RavenQuestionnaire.Core.Views.Answer.CompleteAnswerView.
        /// </returns>
        CompleteAnswerView CreateGroup(CompleteQuestionnaireDocument doc, ICompleteAnswer answer);

        #endregion
    }
}