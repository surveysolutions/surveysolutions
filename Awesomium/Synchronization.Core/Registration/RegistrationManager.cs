using NLog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using Newtonsoft.Json;
using Common.Utils;
using Synchronization.Core.Interface;
using Synchronization.Core.Errors;
using Synchronization.Core.Events;

namespace Synchronization.Core.Registration
{
    public delegate void NewPacketsAvailableHandler(object sender, IList<IAuthorizationPacket> packets);

    /// <summary>
    /// Base class responsible for registraction of CAPI device on Supervisor
    /// </summary>
    public abstract class RegistrationManager
    {
        #region Members

        private IRSACryptoService rsaCryptoService = new RSACryptoService();
        private Guid? currentUser;
        private IRequestProcessor requestProcessor;
        private IUrlUtils urlUtils;
        private IUsbProvider usbProvider;
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private string registrationName = null;
        private Guid? registrationId = null;

        private Authorization authorizationService;
        private bool isCollectionProcessing;

        #endregion

        #region Events

        public event EventHandler<RegistrationEventArgs> RegistrationPhaseStarted;
        public event EventHandler<RegistrationEventArgs> FirstPhaseAccomplished;
        public event EventHandler<RegistrationEventArgs> SecondPhaseAccomplished;

        public event NewPacketsAvailableHandler PacketsAvailable;

        #endregion


        #region C-tor

        protected RegistrationManager(string inFile, string outFile, IRequestProcessor requestProcessor, IUrlUtils urlUtils, IUsbProvider usbProvider)
        {
            InFile = inFile;
            OutFile = outFile;

            this.requestProcessor = requestProcessor;
            this.urlUtils = urlUtils;
            this.usbProvider = usbProvider;

            this.authorizationService = DoInstantiateAuthService(urlUtils, requestProcessor);
            this.authorizationService.PacketsCollected += new AuthorizationPacketsAlarm(registrationService_PacketsCollected);
        }

        protected abstract Authorization DoInstantiateAuthService(IUrlUtils urlUtils, IRequestProcessor requestProcessor);

        #endregion

        public static string SerializeContent<T>(T data)
        {
            var settings = new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.None };
            return JsonConvert.SerializeObject(data, Formatting.Indented, settings);
        }

        public static T DeserializeContent<T>(string data)
        {
            var settings = new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.None };
            return JsonConvert.DeserializeObject<T>(data, settings);
        }

        #region Helpers

        private void CheckPrerequisites(bool firstPhase)
        {
            OnCheckPrerequisites(firstPhase);
        }

        void registrationService_PacketsCollected(IList<IAuthorizationPacket> packets)
        {
            try
            {
                OnAuthorizationPacketsAvailable(packets);

                if (PacketsAvailable != null)
                    PacketsAvailable(this, packets);
            }
            catch(Exception e)
            {
                Logger.Error(e.Message);
            }
        }

        private Guid AcceptRegistrationId()
        {
            try
            {
                if (this.registrationId.HasValue)
                    return this.registrationId.Value;

                this.registrationId = OnAcceptRegistrationId();

                return this.registrationId.Value;
            }
            catch (Exception e)
            {
                Logger.Error(e.Message);
                throw;
            }
        }

        private string AcceptRegistrationName()
        {
            try
            {
                if (!string.IsNullOrEmpty(this.registrationName))
                    return this.registrationName;

                this.registrationName = OnAcceptRegistrationName();

                return this.registrationName;
            }
            catch (Exception e)
            {
                Logger.Error(e.Message);
                throw;
            }
        }

        private string SerializeRegisterData(RegisterData data)
        {
            var settings = new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.None };
            return JsonConvert.SerializeObject(data, Formatting.Indented, settings);
        }

        private RegisterData DeserializeRegisterData(string data)
        {
            var settings = new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.None };
            return JsonConvert.DeserializeObject<RegisterData>(data, settings);
        }

        private byte[] send(string url, byte[] data)
        {
            var request = WebRequest.Create(url);

            request.ContentType = "application/json; charset=utf-8";
            request.Method = "POST";
            request.ContentLength = data.Length;

            using (Stream os = request.GetRequestStream())
            {
                os.Write(data, 0, data.Length);
                os.Close();
            }

            var response = request.GetResponse();
            if (response == null)
                throw new RegistrationException("Data was posted, but there is no response from local host");

            byte[] buffer = new byte[4097];
            using (Stream responseStream = response.GetResponseStream())
            {
                using (MemoryStream memoryStream = new MemoryStream())
                {
                    for (int count = 0; (count = responseStream.Read(buffer, 0, buffer.Length)) > 0; )
                    {
                        memoryStream.Write(buffer, 0, count);
                    }

                    return memoryStream.ToArray();
                }
            }
        }

        private void SendPacket(string url, IAuthorizationPacket packet)
        {
            try
            {
                var registerData = packet.Data as RegisterData;

                var data = Encoding.UTF8.GetBytes(SerializeRegisterData(registerData));
                var response = send(url, data);

                var result = Encoding.UTF8.GetString(response, 0, response.Length);

                if (string.Compare(result, "True", true) != 0)
                    throw new RegistrationFaultException(packet);
            }
            catch (RegistrationException ex)
            {
                Logger.Error(ex.Message);
                throw;
            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message);
                throw new RegistrationFaultException(packet, ex);
            }

        }

        private void WriteRegistrationFile(RegisterData registeredData, string filePath)
        {
            var dataToFile = Encoding.UTF8.GetBytes(SerializeRegisterData(registeredData));

            FileStream fileStream = null;

            try
            {
                // Open file for reading
                fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write);

                // Writes a block of bytes to this stream using data from a byte array.
                fileStream.Write(dataToFile, 0, dataToFile.Length);

            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message);
                throw ex;
            }
            finally
            {
                // close file stream
                if (fileStream != null)
                    fileStream.Close();
            }
        }

        private RegisterData ReadRegistrationFile(string filePath)
        {
            if (!File.Exists(filePath))
                throw new RegistrationException(string.Format("Registration file {0} is not found", filePath));

            FileStream fileStream = null;
            try
            {
                fileStream = File.OpenRead(filePath);
                byte[] data = new byte[fileStream.Length];
                fileStream.Read(data, 0, Convert.ToInt32(fileStream.Length));

                return DeserializeRegisterData(Encoding.UTF8.GetString(data));
            }
            finally
            {
                if (fileStream != null)
                    fileStream.Close();
            }
        }

        private string Register(bool firstPhase, IList<IAuthorizationPacket> packets)
        {
            IList<Exception> errorList = new List<Exception>();

            foreach (var packet in packets)
            {
                try
                {
                    if (firstPhase)
                        StartRegistration(packet);
                    else
                        FinalizeRegistration(packet);
                }
                catch (CancelledServiceException e)
                {
                    Logger.Info(e.Message);
                    throw; // cancel all at once
                }
                catch (Exception e)
                {
                    Logger.Error(e.Message);
                    errorList.Add(e);
                }
            }

            var result = new StringBuilder();
            foreach (var regException in errorList)
                result.AppendLine(regException.Message);

            return result.ToString();
        }

        private IList<IAuthorizationPacket> PrepareAuthorizationPackets(bool firstPhase)
        {
            return OnPrepareAuthorizationPackets(firstPhase, this.authorizationService.NewServicePackets);
        }

        private void StartRegistration(IAuthorizationPacket packet)
        {
            try
            {
                OnStartRegistration(packet);

                packet.IsTreated = true;
            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message);
                throw;
            }
        }

        private void FinalizeRegistration(IAuthorizationPacket packet)
        {
            try
            {
                OnFinalizeRegistration(packet);

                packet.IsTreated = true;
            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message);
                throw;
            }
        }

        #endregion

        protected AuthorizationPacket InstantiatePacket(bool request, ServicePacketChannel channel, RegisterData data = null)
        {
            var keyContainerName = ContainerName;

            var registeredData = data == null ?
                new RegisterData()
                {
                    SecretKey = this.rsaCryptoService.GetPublicKey(keyContainerName).Modulus,
                    RegistrationId = RegistrationId,
                    Description = RegistrationName,
                    RegisterDate = DateTime.Now,
                    Registrator = CurrentUser, // makes not much sence for now, since we do not require CAPI user to be logged on; and on supervisor it coincides with RegistrationId
                } 
                :
                new RegisterData(data);

            return new AuthorizationPacket(registeredData, channel, request ? ServicePacketType.Request : ServicePacketType.Responce);
        }

        #region Properties

        public Guid RegistrationId { get { return AcceptRegistrationId(); } }
        public string RegistrationName { get { return AcceptRegistrationName(); } }

        protected string InFile { get; private set; }
        protected string OutFile { get; private set; }
        
        private string LocalRegistrationController { get { return this.urlUtils.GetRegistrationUrl(); } }
        private string LocalAuthorizationController { get { return this.urlUtils.GetAuthorizationUrl(); } }

        protected Guid CurrentUser
        {
            get
            {
                if (this.currentUser.HasValue)
                    return this.currentUser.Value;

                this.currentUser = this.requestProcessor.Process<Guid>(urlUtils.GetWhoIsCurrentUserUrl(), "GET", true, Guid.Empty);

                return this.currentUser.Value;
            }
        }

        #endregion

        #region Abstract Properties

        protected abstract string ContainerName { get; }

        #endregion

        #region Abstract and Virtual Methods

        protected abstract void OnCheckPrerequisites(bool firstPhase);

        protected abstract IList<IAuthorizationPacket> OnPrepareAuthorizationPackets(bool firstPhase, IList<IAuthorizationPacket> webServicePackets);

        protected abstract Guid OnAcceptRegistrationId();

        protected abstract string OnAcceptRegistrationName();

        protected abstract void OnAuthorizationPacketsAvailable(IList<IAuthorizationPacket> packets);

        protected virtual void OnStartRegistration(IAuthorizationPacket packet)
        {
            if (packet.Channel != ServicePacketChannel.Usb)
                return;

            var usb = this.usbProvider.ActiveUsb;
            if (usb == null)
                throw new UsbNotChoozenException();

            WriteRegistrationFile(packet.Data as RegisterData, usb.Name + OutFile);
        }

        protected virtual void OnFinalizeRegistration(IAuthorizationPacket packet)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region Protected Operations

        /// <summary>
        /// Send authorization request (CAPI) / responce (Supervisor) via net
        /// </summary>
        /// <param name="packet"></param>
        protected bool SendAuthorizationData(IAuthorizationPacket packet)
        {
            if (packet.Channel == ServicePacketChannel.Usb)
                return false;

            SendPacket(LocalAuthorizationController, packet);
            return true;
        }

        /// <summary>
        /// Save registration data in local database
        /// </summary>
        /// <param name="packet"></param>
        protected void AuthorizeAcceptedData(IAuthorizationPacket packet)
        {
            SendPacket(LocalRegistrationController, packet);
        }

        #endregion

        #region Public Methods

        public void DoRegistration(bool firstPhase)
        {
            RegistrationException error = null;
            string log = null;
            IList<IAuthorizationPacket> packets = null;

            try
            {
                packets = PrepareAuthorizationPackets(firstPhase);
                if (packets == null || packets.Count == 0)
                    return;

                if (RegistrationPhaseStarted != null)
                    RegistrationPhaseStarted(this, new RegistrationEventArgs(packets, firstPhase, error));

                CheckPrerequisites(firstPhase);

                log = Register(firstPhase, packets);
            }
            catch (Exception ex)
            {
                error = new RegistrationFaultException(packets, ex);
                log = ex.Message;
            }
            finally
            {
                var regEvent = new RegistrationEventArgs(packets, firstPhase, error);
                if(error == null)
                    regEvent.AppendResultMessage(log);
    
                if (firstPhase)
                {
                    if (FirstPhaseAccomplished != null)
                        FirstPhaseAccomplished(this, regEvent);
                }
                else
                {
                    if (SecondPhaseAccomplished != null)
                        SecondPhaseAccomplished(this, regEvent);
                }
            }
        }

        public IList<Errors.ServiceException> CheckRegIssues()
        {
            IList<ServiceException> errors = new List<ServiceException>();

            var drive = this.usbProvider.ActiveUsb;
            if (drive == null)
            {
                if (this.usbProvider.IsAnyAvailable)
                    errors.Add(new UsbNotChoozenException());
                else
                    errors.Add(new UsbNotPluggedException());
            }

            var netEndPoint = this.urlUtils.GetEnpointUrl();
            if (string.IsNullOrEmpty(netEndPoint))
                errors.Add(new EndpointNotSetException());

            try
            {
                if (this.requestProcessor.Process<string>(this.urlUtils.GetDefaultUrl(), "False") == "False")
                    throw new LocalHosUnreachableException(); // there is no connection to local host

                // test if there is connection to synchronization endpoint
                if (this.requestProcessor.Process<string>(netEndPoint, "False") == "False")
                    throw new NetUnreachableException(netEndPoint);
            }
            catch (Exception ex)
            {
                if (errors == null)
                    errors = new List<ServiceException>();

                errors.Add(new NetIssueException(ex));
            }

            return errors;
        }

        #endregion

        // checks web part and usb for registration requests/responces;
        // it runs by timer
        public void CollectAllAuthorizationPackets()
        {
            try
            {
                if (this.isCollectionProcessing)
                    return;

                this.isCollectionProcessing = true;

                var usbPackets = ReadUsbPackets();

                this.authorizationService.CollectAuthorizationPackets(usbPackets);
            }
            finally
            {
                this.isCollectionProcessing = false;
            }
        }

        protected virtual IList<IAuthorizationPacket> OnReadUsbPackets(bool authorizationRequest)
        {
            // read only single request for now
            // todo: change data file name and read all requests

            var usbPackets = new List<IAuthorizationPacket>();

            var drive = this.usbProvider.ActiveUsb;

            if (drive != null)
            {
                ///// read from usb
                var usbData = ReadRegistrationFile(drive.Name + InFile);

                IAuthorizationPacket packet = InstantiatePacket(authorizationRequest, ServicePacketChannel.Usb, usbData);

                packet.IsAuthorized = !authorizationRequest;

                System.Diagnostics.Debug.Assert(packet != null);

                usbPackets.Add(packet);
            }

            return usbPackets;
        }

        private IList<IAuthorizationPacket> ReadUsbPackets()
        {
            IList<IAuthorizationPacket> exrtraPackets = null;

            try
            {
                exrtraPackets = OnReadUsbPackets(false);
            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message);
            }

            return exrtraPackets;
        }

        public void RemoveUsbChannelPackets()
        {
            this.authorizationService.CleanChannelPackets(ServicePacketChannel.Usb);
        }

        public List<RegisterData> GetAuthorizedIds()
        {
            try
            {
                var url = this.urlUtils.GetAuthorizedIDsUrl(RegistrationId);
                var supervisor = this.requestProcessor.Process<string>(url, "False");

                if (string.Compare(supervisor, "False", true) != 0)
                {
                    var content = RegistrationManager.DeserializeContent<List<RegisterData>>(supervisor);
                    if (content == null)
                        return null;

                    return content;
                }
            }
            catch
            {
            }

            return null;
        }
    }
}
