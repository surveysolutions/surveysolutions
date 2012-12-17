using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Management;
using System.Security.Cryptography;
using System.Text;
using Common.Utils;
using Synchronization.Core.Interface;
using Synchronization.Core.Registration;
using Synchronization.Core.Errors;

namespace Browsing.CAPI.Registration
{
    public class CapiRegistrationManager : RegistrationManager
    {
        private readonly Guid DefaultDevice = new Guid("00000000-0000-0000-0000-111111111111");

        public CapiRegistrationManager(IRequestProcessor requestProcessor, IUrlUtils urlUtils, IUsbProvider usbProvider)
            : base("SupervisorRegistration.register", "CAPIRegistration.register", requestProcessor, urlUtils, usbProvider)
        {
        }

        #region Override Methods

        protected override Authorization DoInstantiateAuthService(IUrlUtils urlUtils, IRequestProcessor requestProcessor)
        {
            return new CAPIAuthorization(urlUtils, requestProcessor, RegistrationId);
        }

        protected override string ContainerName
        {
            get
            {
                return RegistrationId.ToString();
            }
        }

        protected override Guid OnAcceptRegistrationId()
        {
            return GetGuidFromProcessorId(); // bind to processor id
        }

        protected override string OnAcceptRegistrationName()
        {
            return Environment.MachineName;
        }

        /// <summary>
        /// Try to send packet via web firstly. If no luck then try to save it on usb
        /// </summary>
        /// <param name="packet"></param>
        protected override void OnStartRegistration(IAuthorizationPacket packet)
        {
            try
            {
                if (SendAuthorizationData(packet))
                    return;
            }
            catch
            {
            }

            try
            {
                packet.SetChannel(ServicePacketChannel.Usb);
                base.OnStartRegistration(packet);
            }
            catch(Exception e)
            {
                throw new RegistrationException("Registration is impossible via both, net and usb channels", e);
            }
        }

        protected override void OnFinalizeRegistration(IAuthorizationPacket packet)
        {
            System.Diagnostics.Debug.Assert(packet.PacketType == ServicePacketType.Responce);

            // exchange registrar and registrationId
            var regPacket = InstantiatePacket(packet.PacketType == ServicePacketType.Request, packet.Channel, packet.Data as RegisterData);

            regPacket.SetRegistrator(packet.Data.RegistrationId);
            regPacket.SetRegistrationId(packet.Data.Registrator);

            AuthorizeAcceptedData(regPacket);
        }

        protected override void OnAuthorizationPacketsAvailable(IList<IAuthorizationPacket> packets)
        {
            // process automatically only if net channel authorization came
            if(packets.Where(p => p.Channel == ServicePacketChannel.Net).Count() > 0)
                DoRegistration(false);
        }

        protected override IList<IAuthorizationPacket> OnReadUsbPackets(bool authorizationRequest)
        {
            // read responces
            var packets = base.OnReadUsbPackets(false).Where(p => p.Data.RegistrationId == RegistrationId);

            return packets.ToList();
        }

        protected override void OnCheckPrerequisites(bool firstPhase)
        {
        }

        protected override IList<IAuthorizationPacket> OnPrepareAuthorizationPackets(bool firstPhase, IList<IAuthorizationPacket> webServicePackets)
        {
            if (firstPhase) // create authorization request
            {
                // the origin packet in registration scenario is instantiated to be proceeded via net by default
                webServicePackets = new List<IAuthorizationPacket>() { InstantiatePacket(true, ServicePacketChannel.Net) };
            }
            else // treat authorization responce
            {
                if (webServicePackets.Count == 0)
                    throw new RegistrationException("There is no authorization response from supervisor", null);

                // todo: clean the list here
                var newList = new List<IAuthorizationPacket>();

                newList.Add(webServicePackets.OrderBy(p => p.Data.RegisterDate).Last());

                webServicePackets = newList;
            }

            return webServicePackets;
        }

        #endregion

        #region Helpers

        private Guid GetGuidFromProcessorId()
        {
            string cpuInfo = null;
            string selectedDrive = String.Empty;

            var management = new ManagementClass("win32_processor");
            var managementObjects = management.GetInstances();

            foreach (var mo in managementObjects)
            {
                cpuInfo = mo.Properties["processorID"].Value.ToString();
                break;
            }

            if (cpuInfo == null)
                return DefaultDevice;

            foreach (DriveInfo drive in DriveInfo.GetDrives())
            {
                if (!drive.IsReady) continue;
                selectedDrive = drive.RootDirectory.ToString();
                break;
            }

            var disk = new ManagementObject(@"win32_logicaldisk.deviceid=""" + selectedDrive.Substring(0, 1) + @":""");
            disk.Get();
            selectedDrive = disk["VolumeSerialNumber"].ToString();

            using (var md5 = MD5.Create())
            {
                byte[] hash = md5.ComputeHash(Encoding.Default.GetBytes(cpuInfo + selectedDrive));
                var result = new Guid(hash);

                return result;
            }
        }

        #endregion
    }
}
