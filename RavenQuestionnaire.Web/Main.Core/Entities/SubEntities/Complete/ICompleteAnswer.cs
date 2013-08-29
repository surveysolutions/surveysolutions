namespace Main.Core.Entities.SubEntities.Complete
{
    using System;

    /// <summary>
    /// The CompleteAnswer interface.
    /// </summary>
    public interface ICompleteAnswer : IAnswer
    {
        #region Public Properties

        /*/// <summary>
        /// Gets or sets the propogation public key.
        /// </summary>
        Guid? PropogationPublicKey { get; set; }*/

        /// <summary>
        /// Gets or sets a value indicating whether selected.
        /// </summary>
        bool Selected { get; set; }

        #endregion
    }
}