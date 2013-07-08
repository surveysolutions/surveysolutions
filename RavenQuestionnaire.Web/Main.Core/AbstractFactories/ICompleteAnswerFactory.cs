namespace Main.Core.AbstractFactories
{
    using Main.Core.Entities.SubEntities;
    using Main.Core.Entities.SubEntities.Complete;

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
        /// The Main.Core.Entities.SubEntities.Complete.ICompleteAnswer.
        /// </returns>
        ICompleteAnswer ConvertToCompleteAnswer(IAnswer answer);

        #endregion

        /*/// <summary>
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
*/
    }
}