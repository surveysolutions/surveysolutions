// --------------------------------------------------------------------------------------------------------------------
// <copyright file="BaseController.cs" company="">
//   
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
namespace Web.Supervisor.Controllers
{
    using System;
    using System.Web;
    using System.Web.Mvc;

    using Main.Core.View;

    using Ncqrs.Commanding.ServiceModel;

    using Questionnaire.Core.Web.Helpers;

    using WB.UI.Shared.Log;

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
    }
}