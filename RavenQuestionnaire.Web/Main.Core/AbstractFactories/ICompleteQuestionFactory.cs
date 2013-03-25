// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ICompleteQuestionFactory.cs" company="The World Bank">
//   2012
// </copyright>
// <summary>
//   Defines the ICompleteQuestionFactory type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Main.Core.AbstractFactories
{
    using Main.Core.Entities.SubEntities;
    using Main.Core.Entities.SubEntities.Complete;
    using Main.Core.Events.Questionnaire;

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
        AbstractQuestion Create(NewQuestionAdded type);

        /*/// <summary>
        /// The update question by event.
        /// </summary>
        /// <param name="question">
        /// The question.
        /// </param>
        /// <param name="e">
        /// The e.
        /// </param>
        void UpdateQuestionByEvent(IQuestion question, NewQuestionAdded e);*/

        IQuestion CreateQuestionFromExistingUsingDataFromEvent(IQuestion question, QuestionChanged e);

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