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

        public CapiRegistrationManager(IRequesProcessor requestProcessor, IUrlUtils urlUtils, IUsbProvider usbProvider)
            : base("SupervisorRegistration.register", "CAPIRegistration.register", requestProcessor, urlUtils, usbProvider)
        {
        }

        #region Override Methods

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

        protected override void OnStartRegistration(IServiceAuthorizationPacket packet)
        {
            // todo: 1. Try to send packet via network firstly. If no luck, try to save to usb

            base.OnStartRegistration(packet);
        }

        protected override void OnFinalizeRegistration(IServiceAuthorizationPacket packet)
        {
            System.Diagnostics.Debug.Assert(packet.Type == ServicePackectType.Responce);

            AuthorizeAcceptedData(packet);
        }

        protected override void OnNewAuthorizationPacketsAvailable(IList<IServiceAuthorizationPacket> packets)
        {
            // process automatically
            DoRegistration(false);
        }

        protected override IList<IServiceAuthorizationPacket> OnReadUsbPackets(bool authorizationRequest)
        {
            // read responces
            var packets = base.OnReadUsbPackets(false).Where((p) => p.Data.RegistrationId == RegistrationId); 

            return packets.ToList();
        }

        protected override void OnCheckPrerequisites(bool firstPhase)
        {
        }

        protected override IList<IServiceAuthorizationPacket> OnPrepareAuthorizationPackets(bool firstPhase, IList<IServiceAuthorizationPacket> webServicePackets)
        {
            if (firstPhase) // create authorization request
            {
                webServicePackets = new List<IServiceAuthorizationPacket>() { PreparePacket(true, RegistrationId, true) };
            }
            else // treat authorization responce
            {
                // todo: clean the list here
                System.Diagnostics.Debug.Assert(webServicePackets.Count <= 1);

                if (webServicePackets.Count == 0)
                    throw new RegistrationException("There is no authorization response from supervisor", null);
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
            
            var disk = new ManagementObject(@"win32_logicaldisk.deviceid=""" + selectedDrive.Substring(0,1) + @":""");
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
