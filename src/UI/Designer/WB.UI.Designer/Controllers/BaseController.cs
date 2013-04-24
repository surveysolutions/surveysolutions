// --------------------------------------------------------------------------------------------------------------------
// <copyright file="BaseController.cs" company="">
//   
// </copyright>
// <summary>
//   The base controller.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace WB.UI.Designer.Controllers
{
    using System.Web.Mvc;

    using Main.Core.View;

    using Ncqrs.Commanding.ServiceModel;

    using WB.UI.Designer.BootstrapSupport;

    /// <summary>
    ///     The base controller.
    /// </summary>
    public class BaseController : Controller
    {
        #region Fields

        /// <summary>
        ///     The command service.
        /// </summary>
        protected readonly ICommandService CommandService;

        /// <summary>
        ///     The repository.
        /// </summary>
        protected readonly IViewRepository Repository;


        protected readonly IUserHelper UserHelper;

        #endregion

        #region Constructors and Destructors

        public BaseController(IViewRepository repository, ICommandService commandService, IUserHelper userHelper)
        {
            this.Repository = repository;
            this.CommandService = commandService;
            this.UserHelper = userHelper;
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

        /// <summary>
        /// Initializes data that might not be available when the constructor is called.
        /// </summary>
        /// <param name="requestContext">The HTTP context and route data.</param>
        protected override void Initialize(System.Web.Routing.RequestContext requestContext)
        {
            base.Initialize(requestContext);

            ViewBag.UserHelper = UserHelper;
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