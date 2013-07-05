using WB.Core.GenericSubdomains.Logging;

namespace Web.Supervisor.Controllers
{
    using System.Web.Mvc;

    using Ncqrs.Commanding.ServiceModel;

    using Questionnaire.Core.Web.Helpers;
    using Web.Supervisor.Utils;

    public abstract class BaseController : Controller
    {
        protected readonly ICommandService CommandService;
        protected readonly IGlobalInfoProvider GlobalInfo;

        protected readonly ILogger Logger;

        protected BaseController(ICommandService commandService, IGlobalInfoProvider globalInfo, ILogger logger)
        {
            this.CommandService = commandService;
            this.GlobalInfo = globalInfo;
            this.Logger = logger;
        }

        #region Public Methods and Operators

        /// <summary>
        /// The attention.
        /// </summary>
        /// <param name="message">
        /// The message.
        /// </param>
        public void Attention(string message)
        {
            this.WriteToTempData(Alerts.ATTENTION, message);
        }

        /// <summary>
        /// The error.
        /// </summary>
        /// <param name="message">
        /// The message.
        /// </param>
        public void Error(string message)
        {
            this.WriteToTempData(Alerts.ERROR, message);
        }

        /// <summary>
        /// The information.
        /// </summary>
        /// <param name="message">
        /// The message.
        /// </param>
        public void Information(string message)
        {
            this.WriteToTempData(Alerts.INFORMATION, message);
        }

        /// <summary>
        /// The success.
        /// </summary>
        /// <param name="message">
        /// The message.
        /// </param>
        public void Success(string message)
        {
            this.WriteToTempData(Alerts.SUCCESS, message);
        }

        #endregion

        #region Methods

        /// <summary>
        /// The write to temp data.
        /// </summary>
        /// <param name="key">
        /// The key.
        /// </param>
        /// <param name="message">
        /// The message.
        /// </param>
        private void WriteToTempData(string key, string message)
        {
            if (this.TempData.ContainsKey(key))
            {
                this.TempData[key] = message;
            }
            else
            {
                this.TempData.Add(key, message);
            }
        }

        #endregion
    }
}