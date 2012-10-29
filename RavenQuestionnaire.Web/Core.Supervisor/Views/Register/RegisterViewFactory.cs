// -----------------------------------------------------------------------
// <copyright file="RegisterViewFactory.cs" company="WorldBank">
// 2012
// </copyright>
// -----------------------------------------------------------------------

namespace Core.Supervisor.Views.Register
{
    using System.Linq;

    using Main.Core.Documents;
    using Main.Core.View;
    using Main.DenormalizerStorage;

    /// <summary>
    /// TODO: Update summary.
    /// </summary>
    public class RegisterViewFactory : IViewFactory<RegisterInputModel, RegisterView>
    {

        #region Fields

        /// <summary>
        /// field documentItemSession
        /// </summary>
        private readonly IDenormalizerStorage<SyncDeviceRegisterDocument> documentItemSession;

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="RegisterViewFactory"/> class.
        /// </summary>
        /// <param name="item">
        /// The item.
        /// </param>
        public RegisterViewFactory(IDenormalizerStorage<SyncDeviceRegisterDocument> item)
        {
            this.documentItemSession = item;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Load RegisterEvent from denormalizer
        /// </summary>
        /// <param name="input">
        /// The input.
        /// </param>
        /// <returns>
        /// Return RegisterView
        /// </returns>
        public RegisterView Load(RegisterInputModel input)
        {
            var item = this.documentItemSession.Query().Where(t => t.TabletId == input.TabletId).FirstOrDefault();
            if (item == null)
                return new RegisterView(new SyncDeviceRegisterDocument());
            return new RegisterView(item);
        }

        #endregion
    }
}
