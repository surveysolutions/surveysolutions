namespace Web.CAPI.Injections
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;

    using Core.CAPI.Views;


    using Main.Core;
    using Main.Core.Events;
    using Main.Core.View.CompleteQuestionnaire.ScreenGroup;

    using Questionnaire.Core.Web.Security;

    /// <summary>
    /// TODO: Update summary.
    /// </summary>
    public class CAPICoreRegistry : CoreRegistry
    {
        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="CAPICoreRegistry"/> class.
        /// </summary>
        /// <param name="repositoryPath">
        /// The repository path.
        /// </param>
        /// <param name="isEmbeded">
        /// The is embeded.
        /// </param>
        public CAPICoreRegistry(string repositoryPath, bool isEmbeded)
            : base(repositoryPath, isEmbeded)
        {
        }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        /// The get assweblys for register.
        /// </summary>
        /// <returns>
        /// The <see cref="IEnumerable"/>.
        /// </returns>
        public override IEnumerable<Assembly> GetAssweblysForRegister()
        {
            return
                base.GetAssweblysForRegister().Concat(
                    new[] { typeof(QuestionnaireMembershipProvider).Assembly });
        }

        /// <summary>
        /// The load.
        /// </summary>
        public override void Load()
        {
            base.Load();

            this.Unbind<IScreenViewSupplier>();
            this.Bind<IScreenViewSupplier>().To<CapiScreenViewSupplier>();

        }

        #endregion
    }
}