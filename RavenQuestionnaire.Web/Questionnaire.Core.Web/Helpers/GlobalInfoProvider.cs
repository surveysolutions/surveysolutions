namespace Questionnaire.Core.Web.Helpers
{
    using Main.Core.Entities.SubEntities;

    /// <summary>
    /// The global info provider.
    /// </summary>
    public class GlobalInfoProvider : IGlobalInfoProvider
    {
        #region Public Methods and Operators

        /// <summary>
        /// The get current user.
        /// </summary>
        /// <returns>
        /// The ???.
        /// </returns>
        public UserLight GetCurrentUser()
        {
            return GlobalInfo.GetCurrentUser();
        }

        /// <summary>
        /// The is any user exist.
        /// </summary>
        /// <returns>
        /// The <see cref="bool"/>.
        /// </returns>
        public bool IsAnyUserExist()
        {
            return GlobalInfo.IsAnyUserExist();
        }

        public bool IsHeadquarter {
            get
            {
                return GlobalInfo.IsHeadquarter;
            }
        }

        public bool IsSurepvisor {
            get
            {
                return GlobalInfo.IsSupervisor;
            }
        }

        #endregion
    }
}