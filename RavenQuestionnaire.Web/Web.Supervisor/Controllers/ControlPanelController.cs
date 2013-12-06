using System;
using System.ServiceModel;
using System.ServiceModel.Security;
using System.Web.Mvc;
using Microsoft.Practices.ServiceLocation;
using Raven.Client.Linq.Indexing;
using WB.Core.Infrastructure.ReadSide;
using WB.Core.Synchronization;
using Web.Supervisor.DesignerPublicService;
using Web.Supervisor.Models;

namespace Web.Supervisor.Controllers
{
    [AllowAnonymous]
    public class ControlPanelController : Controller
    {
        private readonly IServiceLocator serviceLocator;
        private readonly IIncomePackagesRepository incomePackagesRepository;
        public ControlPanelController(IServiceLocator serviceLocator, IIncomePackagesRepository incomePackagesRepository)
        {
            this.serviceLocator = serviceLocator;
            this.incomePackagesRepository = incomePackagesRepository;
        }

        /// <remarks>
        /// Getting dependency via service location ensures that parts of control panel not using it will always work.
        /// E.g. If Raven connection fails to be established then NConfig info still be available.
        /// </remarks>
        private IReadSideAdministrationService ReadSideAdministrationService
        {
            get { return this.serviceLocator.GetInstance<IReadSideAdministrationService>(); }
        }

        public ActionResult NConfig()
        {
            return this.View();
        }

        public ActionResult Designer()
        {
            return this.Designer(null, null);
        }

        [HttpPost]
        public ActionResult Designer(string login, string password)
        {
            return this.View(model: this.DiagnoseDesignerConnection(login, password));
        }

        public ActionResult IncomingDataWithErrors()
        {
            return this.View(incomePackagesRepository.GetListOfUnhandledPackages());
        }

        public FileResult GetIncomingDataWithError(Guid id)
        {
            return this.File(incomePackagesRepository.GetUnhandledPackagePath(id), System.Net.Mime.MediaTypeNames.Application.Octet);
        }

        public ActionResult ReadLayer()
        {
            return this.RedirectToActionPermanent("ReadSide");
        }

        public ActionResult ReadSide()
        {
            return this.View(this.ReadSideAdministrationService.GetAllAvailableHandlers());
        }

        [OutputCache(NoStore = true, Duration = 0, VaryByParam = "*")]
        public string GetReadSideStatus()
        {
            return this.ReadSideAdministrationService.GetReadableStatus();
        }

        public ActionResult RebuildReadSidePartially(string[] handlers)
        {
            this.ReadSideAdministrationService.RebuildViewsAsync(handlers);
            this.TempData["CheckedHandlers"] = handlers;
            return this.RedirectToAction("ReadSide");
        }

        public ActionResult RebuildReadSide()
        {
            this.ReadSideAdministrationService.RebuildAllViewsAsync();

            return this.RedirectToAction("ReadSide");
        }

        public ActionResult StopReadSideRebuilding()
        {
            this.ReadSideAdministrationService.StopAllViewsRebuilding();

            return this.RedirectToAction("ReadSide");
        }

        private string DiagnoseDesignerConnection(string login, string password)
        {
            if (string.IsNullOrEmpty(login) || string.IsNullOrEmpty(password))
                return "Please provide login and password to continue...";

            var service = new PublicServiceClient();

            service.ClientCredentials.UserName.UserName = login;
            service.ClientCredentials.UserName.Password = password;

            try
            {
                service.Dummy();

                return string.Format("Login to {0} succeeded!", service.Endpoint.Address.Uri);
            }
            catch (Exception exception)
            {
                return string.Format("Login to {1} failed.{0}{0}{2}", Environment.NewLine,
                    service.Endpoint.Address.Uri,
                    FormatDesignerConnectionException(exception));
            }
        }

        private static string FormatDesignerConnectionException(Exception exception)
        {
            var faultException = exception.InnerException as FaultException;

            if (faultException != null)
                return string.Format("Fault code: [ predefined: {1}, sender: {2}, receiver: {3}, subcode: {4}, name: {5} ]{0}{0}{6}", Environment.NewLine,
                    faultException.Code.IsPredefinedFault,
                    faultException.Code.IsSenderFault,
                    faultException.Code.IsReceiverFault,
                    faultException.Code.SubCode,
                    faultException.Code.Name,
                    exception);

            return exception.ToString();
        }
    }
}