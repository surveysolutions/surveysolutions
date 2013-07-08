namespace Main.Core.Documents
{
    using System;
    using System.Collections.Generic;

    using Main.Core.Entities.Extensions;
    using Main.Core.Entities.SubEntities;
    using Main.Core.Entities.SubEntities.Complete;

    /// <summary>
    /// The CompleteQuestionnaireDocument interface.
    /// </summary>
    public interface ICompleteQuestionnaireDocument : IQuestionnaireDocument, ICompleteGroup
    {
        #region Public Properties

        /// <summary>
        /// Gets or sets the creator.
        /// </summary>
        UserLight Creator { get; set; }

        /// <summary>
        /// Gets or sets the responsible.
        /// </summary>
        UserLight Responsible { get; set; }

        /// <summary>
        /// Gets or sets the status.
        /// </summary>
        SurveyStatus Status { get; set; }

        /// <summary>
        /// Gets or sets the template id.
        /// </summary>
        Guid TemplateId { get; set; }

        /// <summary>
        /// The get questions.
        /// </summary>
        /// <returns>
        /// The <see cref="IEnumerable"/>.
        /// </returns>
        IEnumerable<ICompleteQuestion> GetQuestions();

        /// <summary>
        /// The wrapped questions.
        /// </summary>
        /// <returns>
        /// The <see cref="IEnumerable"/>.
        /// </returns>
        IEnumerable<CompleteQuestionWrapper> WrappedQuestions();

        /// <summary>
        /// The get question.
        /// </summary>
        /// <param name="publicKey">
        /// The public key.
        /// </param>
        /// <param name="propagationKey">
        /// The propagation key.
        /// </param>
        /// <returns>
        /// The <see cref="ICompleteQuestion"/>.
        /// </returns>
        ICompleteQuestion GetQuestion(Guid publicKey, Guid? propagationKey);

        /// <summary>
        /// The get question wrapper.
        /// </summary>
        /// <param name="publicKey">
        /// The public key.
        /// </param>
        /// <param name="propagationKey">
        /// The propagation key.
        /// </param>
        /// <returns>
        /// The <see cref="CompleteQuestionWrapper"/>.
        /// </returns>
        CompleteQuestionWrapper GetQuestionWrapper(Guid publicKey, Guid? propagationKey);

        /// <summary>
        /// The get featured questions.
        /// </summary>
        /// <returns>
        /// The <see cref="IEnumerable"/>.
        /// </returns>
        IEnumerable<ICompleteQuestion> GetFeaturedQuestions();


        #endregion
    }
}