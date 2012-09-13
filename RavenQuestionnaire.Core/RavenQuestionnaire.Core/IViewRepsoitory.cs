// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IViewRepsoitory.cs" company="The World Bank">
//   2012
// </copyright>
// <summary>
//   The ViewRepository interface.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace RavenQuestionnaire.Core
{
    /// <summary>
    /// The ViewRepository interface.
    /// </summary>
    public interface IViewRepository
    {
        #region Public Methods and Operators

        /// <summary>
        /// The load.
        /// </summary>
        /// <param name="input">
        /// The input.
        /// </param>
        /// <typeparam name="TInput">
        /// </typeparam>
        /// <typeparam name="TOutput">
        /// </typeparam>
        /// <returns>
        /// The TOutput.
        /// </returns>
        TOutput Load<TInput, TOutput>(TInput input);

        #endregion
    }
}