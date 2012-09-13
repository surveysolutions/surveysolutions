// --------------------------------------------------------------------------------------------------------------------
// <copyright file="GlobalInfoProvider.cs" company="">
//   
// </copyright>
// <summary>
//   The global info provider.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Questionnaire.Core.Web.Helpers
{
    using Main.Core.Entities.SubEntities;

    /// <summary>
    /// The global info provider.
    /// </summary>
    public class GlobalInfoProvider : IGlobalInfoProvider
    {
        #region Public Methods and Operators

        /// <summary>
        /// The get current user.
        /// </summary>
        /// <returns>
        /// The ???.
        /// </returns>
        public UserLight GetCurrentUser()
        {
            return GlobalInfo.GetCurrentUser();
        }

        #endregion
    }
}