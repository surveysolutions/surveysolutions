namespace Main.Core.AbstractFactories
{
    using System;

    using Main.Core.Entities.SubEntities;
    using Main.Core.Entities.SubEntities.Complete;

    /// <summary>
    /// Factory creates ICompleteGroup.
    /// </summary>
    public class CompleteGroupFactory : ICompleteGroupFactory
    {
        #region Public Methods and Operators

        /// <summary>
        /// The convert to complete group.
        /// </summary>
        /// <param name="group">
        /// The group.
        /// </param>
        /// <returns>
        /// The Main.Core.Entities.SubEntities.Complete.ICompleteGroup.
        /// </returns>
        /// <exception cref="ArgumentException">
        /// Rises ArgumentException.
        /// </exception>
        public ICompleteGroup ConvertToCompleteGroup(IGroup group)
        {
            var result = group as ICompleteGroup;
            if (result != null)
            {
                return result;
            }

            var simpleGroup = group as Group;
            if (simpleGroup != null)
            {
                return (CompleteGroup)simpleGroup;
            }

            throw new ArgumentException();
        }

        #endregion
    }
}