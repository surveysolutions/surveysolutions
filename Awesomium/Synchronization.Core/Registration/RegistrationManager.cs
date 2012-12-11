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
    /// <summary>
    /// Base class responsible for registraction of CAPI device on Supervisor
    /// </summary>
    public abstract class RegistrationManager
    {
        #region Members

        private IRSACryptoService rsaCryptoService = new RSACryptoService();
        private Guid? currentUser;
        private IRequesProcessor requestProcessor;
        private IUrlUtils urlUtils;
        private IUsbProvider usbProvider;
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private string registrationName = null;
        private Guid? registrationId = null;

        private SupervisorServiceClient registrationService;

        #endregion

        #region Events

        public event EventHandler<RegistrationEventArgs> RegistrationPhaseStarted;
        public event EventHandler<RegistrationEventArgs> FirstPhaseAccomplished;
        public event EventHandler<RegistrationEventArgs> SecondPhaseAccomplished;

        #endregion

        #region C-tor

        protected RegistrationManager(string inFile, string outFile, IRequesProcessor requestProcessor, IUrlUtils urlUtils, IUsbProvider usbProvider)
        {
            InFile = inFile;
            OutFile = outFile;

            this.requestProcessor = requestProcessor;
            this.urlUtils = urlUtils;
            this.usbProvider = usbProvider;

            this.registrationService = new SupervisorServiceClient(urlUtils);
            this.registrationService.NewPacketsAvailable += new AuthorizationPacketsAlarm(registrationService_NewPacketAvailable);
        }

        #endregion

        public string SerializeContent<T>(T data)
        {
            var settings = new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.None };
            return JsonConvert.SerializeObject(data, Formatting.Indented, settings);
        }

        public T DeserializeContent<T>(string data)
        {
            var settings = new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.None };
            return JsonConvert.DeserializeObject<T>(data, settings);
        }

        #region Helpers

        private void CheckPrerequisites(bool firstPhase)
        {
            OnCheckPrerequisites(firstPhase);
        }

        void registrationService_NewPacketAvailable(IList<IServiceAuthorizationPacket> packets)
        {
            try
            {
                OnNewAuthorizationPacketsAvailable(packets);
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

        private byte[] SendLocalRegistrationRequest(byte[] requestParams)
        {
            string url = LocalRegistrationService;

            //return this.requestProcessor.Process<byte[]>(url, "POST", false, null);

            var request = WebRequest.Create(url);

            request.ContentType = "application/json; charset=utf-8";
            request.Method = "POST";
            request.ContentLength = requestParams.Length;

            using (Stream os = request.GetRequestStream())
            {
                os.Write(requestParams, 0, requestParams.Length);
                os.Close();
            }

            var response = request.GetResponse();

            if (response == null)
                return null;

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

        private void WriteRegistrationFile(RegisterData registeredData, string filePath)
        {
            var dataToFile = Encoding.ASCII.GetBytes(SerializeRegisterData(registeredData));

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
                throw new Exception("Registration file is not found");

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

        private string Register(bool firstPhase, IList<IServiceAuthorizationPacket> packets)
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

        private IList<IServiceAuthorizationPacket> PrepareAuthorizationPackets(bool firstPhase, IList<IServiceAuthorizationPacket> webServicePackets)
        {
            return OnPrepareAuthorizationPackets(firstPhase, webServicePackets);
        }

        private void StartRegistration(IServiceAuthorizationPacket packet)
        {
            try
            {
                OnStartRegistration(packet);
            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message);
                throw;
            }
        }

        private void FinalizeRegistration(IServiceAuthorizationPacket packet)
        {
            try
            {
                OnFinalizeRegistration(packet);
            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message);
                throw;
            }
        }

        #endregion

        protected IServiceAuthorizationPacket PreparePacket(bool request, Guid registrationId, ServicePacketChannel channel, RegisterData data = null)
        {
            var keyContainerName = ContainerName;

            var registeredData = data ?? new RegisterData
            {
                SecretKey = this.rsaCryptoService.GetPublicKey(keyContainerName).Modulus,
                RegistrationId = RegistrationId,
                Description = RegistrationName,
                RegisterDate = DateTime.Now,
                Registrator = CurrentUser, // makes not much sence for now, since we do not require CAPI user to be logged on; and on supervisor it coincides with RegistrationId
            };

            return request ?
                new AuthorizationRequest(registeredData, channel) as IServiceAuthorizationPacket:
                new AuthorizationResponce(registeredData, channel);
        }

        #region Properties

        public Guid RegistrationId { get { return AcceptRegistrationId(); } }
        public string RegistrationName { get { return AcceptRegistrationName(); } }

        protected string InFile { get; private set; }
        protected string OutFile { get; private set; }
        protected string LocalRegistrationService { get { return this.urlUtils.GetRegistrationCapiPath(); } }

        protected Guid CurrentUser
        {
            get
            {
                if (this.currentUser.HasValue)
                    return this.currentUser.Value;

                this.currentUser = this.requestProcessor.Process<Guid>(urlUtils.GetCurrentUserGetUrl(), "GET", true, Guid.Empty);

                return this.currentUser.Value;
            }
        }

        #endregion

        #region Abstract Properties

        protected abstract string ContainerName { get; }

        #endregion

        #region Abstract and Virtual Methods

        protected abstract void OnCheckPrerequisites(bool firstPhase);

        protected abstract IList<IServiceAuthorizationPacket> OnPrepareAuthorizationPackets(bool firstPhase, IList<IServiceAuthorizationPacket> webServicePackets);

        protected abstract Guid OnAcceptRegistrationId();

        protected abstract string OnAcceptRegistrationName();

        protected abstract void OnNewAuthorizationPacketsAvailable(IList<IServiceAuthorizationPacket> packets);

        protected virtual void OnStartRegistration(IServiceAuthorizationPacket packet)
        {
            if (packet.Channel != ServicePacketChannel.Usb)
                return;

            var usb = this.usbProvider.ActiveUsb;
            if (usb == null)
                throw new UsbNotChoozenException();

            var keyContainerName = ContainerName;

            var registeredData = new RegisterData
            {
                SecretKey = this.rsaCryptoService.GetPublicKey(keyContainerName).Modulus,
                RegistrationId = packet.Data.RegistrationId,
                Description = RegistrationName,
                RegisterDate = DateTime.Now,
                Registrator = CurrentUser, // makes not much sence for now, since we do not require CAPI user to be logged on; and on supervisor it coincides with RegistrationId
            };

            WriteRegistrationFile(registeredData, usb.Name + OutFile);

            packet.IsAuthorized = true;
        }

        protected virtual void OnFinalizeRegistration(IServiceAuthorizationPacket packet)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region Protected Operations

        protected void AuthorizeAcceptedData(IServiceAuthorizationPacket packet)
        {
            try
            {
                var registerData = packet.Data;

                // assign registrator id to save in local db
                registerData.Registrator = CurrentUser;

                var data = Encoding.UTF8.GetBytes(SerializeRegisterData(registerData));
                var response = SendLocalRegistrationRequest(data);

                var result = Encoding.UTF8.GetString(response, 0, response.Length);

                if (string.Compare(result, "True", true) != 0)
                    throw new RegistrationFaultException(packet);

                packet.IsAuthorized = true;
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

        #endregion

        #region Public Methods

        public void DoRegistration(bool firstPhase)
        {
            RegistrationException error = null;
            string log = null;
            IList<IServiceAuthorizationPacket> packets = null;

            try
            {
                packets = PrepareAuthorizationPackets(firstPhase, this.registrationService.ServicePackets);
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
                errors.Add(new InactiveNetServiceException());

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

                errors.Add(new NetUnreachableException(ex.Message));
            }

            return errors;
        }

        #endregion

        // check web part and usb for registration requests/responces
        public void CollectAuthorizationPackets()
        {
            var usbPackets = ReadUsbPackets();

            this.registrationService.CollectAuthorizationPackets(usbPackets);
        }

        protected virtual IList<IServiceAuthorizationPacket> OnReadUsbPackets(bool authorizationRequest)
        {
            // read only single request for now
            // todo: change data file name and read all requests

            var usbPackets = new List<IServiceAuthorizationPacket>();

            var drive = this.usbProvider.ActiveUsb;

            if (drive != null)
            {
                ///// read from usb
                var usbData = ReadRegistrationFile(drive.Name + InFile);

                IServiceAuthorizationPacket packet = PreparePacket(authorizationRequest, Guid.Empty, ServicePacketChannel.Usb, usbData);

                System.Diagnostics.Debug.Assert(packet != null);

                usbPackets.Add(packet);
            }

            return usbPackets;
        }

        private IList<IServiceAuthorizationPacket> ReadUsbPackets()
        {
            IList<IServiceAuthorizationPacket> exrtraPackets = null;

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
            this.registrationService.Clean(ServicePacketChannel.Usb);
        }
    }
}
