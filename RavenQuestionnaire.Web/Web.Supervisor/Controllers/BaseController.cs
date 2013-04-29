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
        protected BaseController(IViewRepository repository, ICommandService commandService, IGlobalInfoProvider globalInfo)
        {
            this.Repository = repository;
            this.CommandService = commandService;
            this.GlobalInfo = globalInfo;
        }

        /// <summary>
        /// The parse key or throw 404.
        /// </summary>
        /// <param name="id">
        /// The id.
        /// </param>
        /// <returns>
        /// The <see cref="Guid"/>.
        /// </returns>
        /// <exception cref="HttpException">
        /// </exception>
        protected Guid ParseKeyOrThrow404(string id)
        {
            Guid key;

            if (!Guid.TryParse(id, out key))
            {
                throw new HttpException("404");
            }

            return key;
        }

        #endregion
    }
}