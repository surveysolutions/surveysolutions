// --------------------------------------------------------------------------------------------------------------------
// <copyright file="LogManager.cs" company="">
//   
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
namespace WB.UI.Shared.NLog
{
    using Microsoft.Practices.ServiceLocation;

    using WB.UI.Shared.Log;

    /// <summary>
    ///     The log manager.
    /// </summary>
    public class LogManager
    {
        #region Public Properties

        /// <summary>
        /// Gets the logger.
        /// </summary>
        public static ILog Logger
        {
            get
            {
                return ServiceLocator.Current.GetInstance<ILog>();
            }
        }

        #endregion
    }
}