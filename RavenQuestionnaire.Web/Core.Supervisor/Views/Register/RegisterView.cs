// -----------------------------------------------------------------------
// <copyright file="RegisterView.cs" company="WorldBank">
// 2012
// </copyright>
// -----------------------------------------------------------------------

namespace Core.Supervisor.Views.Register
{
    using Main.Core.Documents;

    /// <summary>
    /// TODO: Update summary.
    /// </summary>
    public class RegisterView
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RegisterView"/> class.
        /// </summary>
        /// <param name="item">
        /// The item.
        /// </param>
        public RegisterView(SyncDeviceRegisterDocument item)
        {
            this.Item = item;
        }

        #region Public Properties

        /// <summary>
        /// Gets or sets the items.
        /// </summary>
        public SyncDeviceRegisterDocument Item { get; set; }

        #endregion

    }
}
