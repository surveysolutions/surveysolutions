namespace Main.Core.Entities.SubEntities.Complete
{
    using System;

    /// <summary>
    /// The DateTimeQuestion interface.
    /// </summary>
    public interface IDateTimeQuestion 
    {
        #region Public Properties

        /// <summary>
        /// Gets or sets the add date time attr.
        /// </summary>
        string AddDateTimeAttr { get; set; }

        /// <summary>
        /// Gets or sets the date time attr.
        /// </summary>
        DateTime DateTimeAttr { get; set; }

        #endregion
    }
}