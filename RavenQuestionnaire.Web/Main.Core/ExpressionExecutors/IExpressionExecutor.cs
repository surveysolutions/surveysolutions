namespace Main.Core.ExpressionExecutors
{
    /// <summary>
    /// The ExpressionExecutor interface.
    /// </summary>
    /// <typeparam name="TInput">
    /// </typeparam>
    /// <typeparam name="TOutput">
    /// </typeparam>
    public interface IExpressionExecutor<in TInput, out TOutput>
    {
        #region Public Methods and Operators

        /// <summary>
        /// The execute.
        /// </summary>
        /// <param name="entity">
        /// The entity.
        /// </param>
        /// <param name="condition">
        /// The condition.
        /// </param>
        /// <returns>
        /// The TOutput.
        /// </returns>
        TOutput Execute(TInput entity, string condition);

        #endregion
    }
}