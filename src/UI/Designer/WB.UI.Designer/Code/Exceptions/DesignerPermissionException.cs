namespace WB.UI.Designer.Code.Exceptions
{
    using System;

    /// <summary>
    /// The designer permission exception.
    /// </summary>
    public class DesignerPermissionException : Exception
    {
        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="DesignerPermissionException"/> class.
        /// </summary>
        public DesignerPermissionException()
            : base("Access Denied")
        {
        }

        #endregion
    }
}