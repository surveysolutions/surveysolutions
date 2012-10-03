// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ICompleteQuestionFactory.cs" company="">
//   
// </copyright>
// <summary>
//   Defines the ICompleteQuestionFactory type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace Main.Core.AbstractFactories
{
    using Main.Core.Entities.SubEntities;
    using Main.Core.Entities.SubEntities.Complete;

    /// <summary>
    /// The CompleteQuestionFactory interface.
    /// </summary>
    public interface ICompleteQuestionFactory
    {
        #region Public Methods and Operators

        /// <summary>
        /// The convert to complete question.
        /// </summary>
        /// <param name="question">
        /// The question.
        /// </param>
        /// <returns>
        /// The Main.Core.Entities.SubEntities.Complete.ICompleteQuestion.
        /// </returns>
        ICompleteQuestion ConvertToCompleteQuestion(IQuestion question);

        /// <summary>
        /// The create.
        /// </summary>
        /// <param name="type">
        /// The type.
        /// </param>
        /// <returns>
        /// The Main.Core.Entities.SubEntities.AbstractQuestion.
        /// </returns>
        AbstractQuestion Create(QuestionType type);

        #endregion

        /*/// <summary>
        /// The create question.
        /// </summary>
        /// <param name="doc">
        /// The doc.
        /// </param>
        /// <param name="question">
        /// The question.
        /// </param>
        /// <returns>
        /// The RavenQuestionnaire.Core.Views.Question.CompleteQuestionView.
        /// </returns>
        CompleteQuestionView CreateQuestion(CompleteQuestionnaireStoreDocument doc, ICompleteQuestion question);
*/
    }
}