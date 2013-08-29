using WB.Core.Infrastructure;
using WB.Core.Infrastructure.ReadSide;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;

namespace Main.Core.View.Register
{
    using System.Linq;

    using Main.Core.Documents;
    using Main.DenormalizerStorage;

    public class RegisterViewFactory : IViewFactory<RegisterInputModel, RegisterView>
    {

        #region Fields

        /// <summary>
        /// field documentItemSession
        /// </summary>
        private readonly IQueryableReadSideRepositoryReader<SyncDeviceRegisterDocument> documentItemSession;

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="RegisterViewFactory"/> class.
        /// </summary>
        /// <param name="item">
        /// The item.
        /// </param>
        public RegisterViewFactory(IQueryableReadSideRepositoryReader<SyncDeviceRegisterDocument> item)
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
            var item = this.documentItemSession.Query(_ => _.Where(t => t.TabletId == input.TabletId).FirstOrDefault());
            if (item == null) 
                return new RegisterView(new SyncDeviceRegisterDocument());
            return new RegisterView(item);
        }

        #endregion
    }
}
