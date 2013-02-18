// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DeviceController.cs" company="">
//   2012
// </copyright>
// <summary>
//   The device controller.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using System;
using System.Web.Mvc;
using Main.Core.Commands.Synchronization;
using Ncqrs;
using Ncqrs.Commanding.ServiceModel;
using Questionnaire.Core.Web.Register;

namespace Web.CAPI.Controllers
{
    /// <summary>
    /// The device controller.
    /// </summary>
    public class DeviceController : Controller
    {
        #region Public Methods and Operators

        /// <summary>
        /// Register CAPI device in supervisor db
        /// </summary>
        /// <param name="data">
        /// Register data object
        /// </param>
        /// <returns>
        /// True on success, false otherwise
        /// </returns>
        public bool RegisterSupervisor(RegisterData data)
        {
            try
            {
                var commandService = NcqrsEnvironment.Get<ICommandService>();
                commandService.Execute(new RegisterDeviceCommand(data.Description, data.SecretKey, data.TabletId));
            }
            catch (Exception)
            {
                return false;
            }

            return true;
        }

        #endregion
    }
}