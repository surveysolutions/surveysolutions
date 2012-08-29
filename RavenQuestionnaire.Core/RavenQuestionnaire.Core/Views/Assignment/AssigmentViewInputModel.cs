// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AssigmentViewInputModel.cs" company="The World Bank">
//   2012
// </copyright>
// <summary>
//   The assigment view input model.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace RavenQuestionnaire.Core.Views.Assignment
{
    /// <summary>
    /// The assigment view input model.
    /// </summary>
    public class AssigmentViewInputModel
    {
        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="AssigmentViewInputModel"/> class.
        /// </summary>
        /// <param name="Id">
        /// The id.
        /// </param>
        public AssigmentViewInputModel(string Id)
        {
            this.Id = Id;
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets or sets the id.
        /// </summary>
        public string Id { get; set; }

        #endregion
    }
}