using Main.Core.Documents;

namespace Main.Core.View.Register
{
    /// <summary>
    /// view register
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
