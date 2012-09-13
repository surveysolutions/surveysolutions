// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IBagManager.cs" company="">
//   
// </copyright>
// <summary>
//   The BagManager interface.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Questionnaire.Core.Web.Helpers
{
    using RavenQuestionnaire.Core;

    /// <summary>
    /// The BagManager interface.
    /// </summary>
    public interface IBagManager
    {
        #region Public Methods and Operators

        /// <summary>
        /// The add users to bag.
        /// </summary>
        /// <param name="bag">
        /// The bag.
        /// </param>
        /// <param name="viewRepository">
        /// The view repository.
        /// </param>
        void AddUsersToBag(dynamic bag, IViewRepository viewRepository);

        #endregion
    }
}