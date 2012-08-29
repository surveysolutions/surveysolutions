// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ICompleteQuestionnaireUploaderService.cs" company="The World Bank">
//   2012
// </copyright>
// <summary>
//   The CompleteQuestionnaireUploaderService interface.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace RavenQuestionnaire.Core.Services
{
    using System;

    using RavenQuestionnaire.Core.Entities;
    using RavenQuestionnaire.Core.Entities.SubEntities;

    /// <summary>
    /// The CompleteQuestionnaireUploaderService interface.
    /// </summary>
    public interface ICompleteQuestionnaireUploaderService
    {
        #region Public Methods and Operators

        /// <summary>
        /// The add comments.
        /// </summary>
        /// <param name="id">
        /// The id.
        /// </param>
        /// <param name="publicKey">
        /// The public key.
        /// </param>
        /// <param name="propagationKey">
        /// The propagation key.
        /// </param>
        /// <param name="comments">
        /// The comments.
        /// </param>
        void AddComments(string id, Guid publicKey, Guid? propagationKey, string comments);

        /// <summary>
        /// The add complete answer.
        /// </summary>
        /// <param name="id">
        /// The id.
        /// </param>
        /// <param name="questionKey">
        /// The question key.
        /// </param>
        /// <param name="propagationKey">
        /// The propagation key.
        /// </param>
        /// <param name="answers">
        /// The answers.
        /// </param>
        /// <returns>
        /// The RavenQuestionnaire.Core.Entities.CompleteQuestionnaire.
        /// </returns>
        CompleteQuestionnaire AddCompleteAnswer(string id, Guid questionKey, Guid? propagationKey, object answers);

        /// <summary>
        /// The create complete questionnaire.
        /// </summary>
        /// <param name="questionnaire">
        /// The questionnaire.
        /// </param>
        /// <param name="completeQuestionnaireGuid">
        /// The complete questionnaire guid.
        /// </param>
        /// <param name="user">
        /// The user.
        /// </param>
        /// <param name="status">
        /// The status.
        /// </param>
        /// <returns>
        /// The RavenQuestionnaire.Core.Entities.CompleteQuestionnaire.
        /// </returns>
        CompleteQuestionnaire CreateCompleteQuestionnaire(
            Questionnaire questionnaire, Guid completeQuestionnaireGuid, UserLight user, SurveyStatus status);

        /// <summary>
        /// The delete complete questionnaire.
        /// </summary>
        /// <param name="id">
        /// The id.
        /// </param>
        void DeleteCompleteQuestionnaire(string id);

        /// <summary>
        /// The propagate group.
        /// </summary>
        /// <param name="id">
        /// The id.
        /// </param>
        /// <param name="publicKey">
        /// The public key.
        /// </param>
        /// <param name="groupPublicKey">
        /// The group public key.
        /// </param>
        void PropagateGroup(string id, Guid publicKey, Guid groupPublicKey);

        /// <summary>
        /// The remove propagated group.
        /// </summary>
        /// <param name="id">
        /// The id.
        /// </param>
        /// <param name="publicKey">
        /// The public key.
        /// </param>
        /// <param name="propagationKey">
        /// The propagation key.
        /// </param>
        void RemovePropagatedGroup(string id, Guid publicKey, Guid propagationKey);

        #endregion
    }
}