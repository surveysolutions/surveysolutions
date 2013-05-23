// --------------------------------------------------------------------------------------------------------------------
// <copyright file="BaseController.cs" company="">
//   
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
namespace Web.Supervisor.Controllers
{
    using System.Web.Mvc;

    using Main.Core.View;

    using Ncqrs.Commanding.ServiceModel;

    using Questionnaire.Core.Web.Helpers;

    using WB.Core.SharedKernel.Logger;

    using Web.Supervisor.Utils;

    /// <summary>
    ///     The base controller.
    /// </summary>
    public abstract class BaseController : Controller
    {
        #region Fields

        /// <summary>
        /// The command service.
        /// </summary>
        protected readonly ICommandService CommandService;

        /// <summary>
        ///     View Repository object
        /// </summary>
        protected readonly IViewRepository Repository;

        /// <summary>
        /// The global info.
        /// </summary>
        protected readonly IGlobalInfoProvider GlobalInfo;

        protected readonly ILog Logger;
        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="BaseController"/> class.
        /// </summary>
        /// <param name="repository">
        /// The repository.
        /// </param>
        /// <param name="commandService">
        /// The command Service.
        /// </param>
        /// <param name="globalInfo">
        /// The global Info.
        /// </param>
        protected BaseController(IViewRepository repository, ICommandService commandService, IGlobalInfoProvider globalInfo, ILog logger)
        {
            this.Repository = repository;
            this.CommandService = commandService;
            this.GlobalInfo = globalInfo;
            this.Logger = logger;
        }

        #endregion

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