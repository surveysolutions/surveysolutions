// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IGlobalInfoProvider.cs" company="">
//   
// </copyright>
// <summary>
//   The GlobalInfoProvider interface.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Questionnaire.Core.Web.Helpers
{
    using Main.Core.Entities.SubEntities;

    /// <summary>
    /// The GlobalInfoProvider interface.
    /// </summary>
    public interface IGlobalInfoProvider
    {
        #region Public Methods and Operators

        /// <summary>
        /// The get current user.
        /// </summary>
        /// <returns>
        /// The ???.
        /// </returns>
        UserLight GetCurrentUser();

        /// <summary>
        /// The is any user exist.
        /// </summary>
        /// <returns>
        /// The <see cref="bool"/>.
        /// </returns>
        bool IsAnyUserExist();

        bool IsHeadquarter { get; }

        bool IsSurepvisor { get; }

        #endregion
    }
}