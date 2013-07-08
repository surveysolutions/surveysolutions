namespace Main.Core.AbstractFactories
{
    using System;

    using Main.Core.Entities.SubEntities;
    using Main.Core.Entities.SubEntities.Complete;

    /// <summary>
    /// The complete answer factory.
    /// </summary>
    public class CompleteAnswerFactory : ICompleteAnswerFactory
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
        /// <exception cref="ArgumentException">
        /// </exception>
        public ICompleteAnswer ConvertToCompleteAnswer(IAnswer answer)
        {
            var simpleAnswer = answer as Answer;
            if (simpleAnswer != null)
            {
                return (CompleteAnswer)simpleAnswer;
            }

            throw new ArgumentException();
        }

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
        /// <exception cref="NotImplementedException">
        /// </exception>
        public CompleteAnswerView CreateGroup(CompleteQuestionnaireDocument doc, ICompleteAnswer answer)
        {
            throw new NotImplementedException();
        }*/
    }
}