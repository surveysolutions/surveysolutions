namespace Main.Core.Entities.SubEntities.Complete
{
    /// <summary>
    /// The CompleteGroup interface.
    /// </summary>
    public interface ICompleteGroup : IGroup, ICompleteItem
    {
        /// <summary>
        /// Is any child item is visible in given scope
        /// </summary>
        /// <param name="questionScope">
        /// The question scope.
        /// </param>
        /// <returns>
        /// True if any children in group tree is visible for given scope
        /// </returns>
        bool HasVisibleItemsForScope(QuestionScope questionScope);
    }
}