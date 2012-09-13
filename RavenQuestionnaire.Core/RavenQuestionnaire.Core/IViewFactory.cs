// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IViewFactory.cs" company="The World Bank">
//   2012
// </copyright>
// <summary>
//   The ViewFactory interface.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace RavenQuestionnaire.Core
{
    /// <summary>
    /// The ViewFactory interface.
    /// </summary>
    /// <typeparam name="TInput">
    /// </typeparam>
    /// <typeparam name="TOutput">
    /// </typeparam>
    public interface IViewFactory<TInput, TOutput>
    {
        #region Public Methods and Operators

        /// <summary>
        /// The load.
        /// </summary>
        /// <param name="input">
        /// The input.
        /// </param>
        /// <returns>
        /// The TOutput.
        /// </returns>
        TOutput Load(TInput input);

        #endregion
    }
}