// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DisplayViewInputModel.cs" company="The World Bank">
//   2012
// </copyright>
// <summary>
//   The complete questionnaire view input model.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Core.Supervisor.Views.Survey
{
    using System;

    using Main.Core.Entities.SubEntities;

    /// <summary>
    /// The complete questionnaire view input model.
    /// </summary>
    public class DisplayViewInputModel
    {

        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="DisplayViewInputModel"/> class.
        /// </summary>
        public DisplayViewInputModel()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DisplayViewInputModel"/> class.
        /// </summary>
        /// <param name="id">
        /// The id.
        /// </param>
        public DisplayViewInputModel(Guid id)
        {
            this.CompleteQuestionnaireId = id;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DisplayViewInputModel"/> class.
        /// </summary>
        /// <param name="id">
        /// The id.
        /// </param>
        /// <param name="groupKey">
        /// The group key.
        /// </param>
        /// <param name="propagationKey">
        /// The propagation key.
        /// </param>
        /// <param name="user">
        /// The user.
        /// </param>
        public DisplayViewInputModel(Guid id, Guid groupKey, Guid? propagationKey, UserLight user)
        {
            this.User = user;
            this.CompleteQuestionnaireId = id;
            this.CurrentGroupPublicKey = groupKey;
            this.PropagationKey = propagationKey;
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets the complete questionnaire id.
        /// </summary>
        public Guid CompleteQuestionnaireId { get; private set; }

        /// <summary>
        /// Gets User.
        /// </summary>
        public UserLight User { get; set; }

        /// <summary>
        /// Gets or sets the current group public key.
        /// </summary>
        public Guid? CurrentGroupPublicKey { get; set; }

        /// <summary>
        /// Gets a value indicating whether is reverse.
        /// </summary>
        public bool IsReverse { get; private set; }

        /// <summary>
        /// Gets or sets the propagation key.
        /// </summary>
        public Guid? PropagationKey { get; set; }

        #endregion
    }
}