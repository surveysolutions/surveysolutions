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

        private string registrationName = null;
        private Guid? registrationId = null;

        #endregion

        #region C-tor

        protected RegistrationManager(string inFile, string outFile, IRequesProcessor requestProcessor, IUrlUtils urlUtils, IUsbProvider usbProvider)
        {
            InFile = inFile;
            OutFile = outFile;

            this.requestProcessor = requestProcessor;
            this.urlUtils = urlUtils;
            this.usbProvider = usbProvider;
        }

        #endregion

        #region Helpers

        internal Guid GetCurrentUser()
        {
            if (this.currentUser.HasValue)
                return this.currentUser.Value;

            this.currentUser = this.requestProcessor.Process<Guid>(urlUtils.GetCurrentUserGetUrl(), "GET", true, Guid.Empty);

            return this.currentUser.Value;
        }

        #endregion

        #region Properties

        public Guid RegisrationId { get { return AcceptRegistrationId(); } }
        public string RegisrationName { get { return AcceptRegistrationName();} }

        protected string InFile { get; private set; }
        protected string OutFile { get; private set; }
        protected string RegistrationService { get { return this.urlUtils.GetRegistrationCapiPath(); } }

        #endregion

        #region Virtual Properties
        
        protected virtual string ContainerName
        {
            get { return GetCurrentUser().ToString(); }
        }

        #endregion

        #region Helpers

        private Guid AcceptRegistrationId()
        {
            try
            {
                if (this.registrationId.HasValue)
                    return this.registrationId.Value;

                this.registrationId = OnAcceptRegistrationId();

                return this.registrationId.Value;
            }
            catch // todo: log
            {
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
            catch // todo: log
            {
                throw;
            }
        }


        #endregion


        #region Abstract and Virtual Methods

        protected virtual Guid OnAcceptRegistrationId()
        {
            return GetCurrentUser();
        }

        protected virtual string OnAcceptRegistrationName()
        {
            return string.Format("supervisor #'{0}'", AcceptRegistrationId().ToString()); // todo: replace with true name
        }

        protected virtual bool OnStartRegistration(string folderPath, out Registration.RegisterData registeredData)
        {
            var keyContainerName = ContainerName;

            registeredData = new RegisterData { SecretKey = this.rsaCryptoService.GetPublicKey(keyContainerName).Modulus, RegisterId = RegisrationId, Description = RegisrationName };

            var dataToFile = Encoding.ASCII.GetBytes(SerializeRegisterData(registeredData));

            return CreateRegistrationFile(dataToFile, folderPath + OutFile);
        }

        protected abstract bool OnFinalizeRegistration(string folderPath, out Registration.RegisterData registeredData);

        #endregion

        #region Protected Operations

        protected string SerializeRegisterData(RegisterData data)
        {
            var settings = new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.None };
            return JsonConvert.SerializeObject(data, Formatting.Indented, settings);
        }

        protected RegisterData DeserializeRegisterData(string data)
        {
            var settings = new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.None };
            return JsonConvert.DeserializeObject<RegisterData>(data, settings);
        }

        protected byte[] SendRegistrationRequest(RegisterData reqParams)
        {
            string url = RegistrationService;
            //return this.requestProcessor.Process<byte[]>(url, "POST", false, null);
            reqParams.GuidCurrentUser = this.GetCurrentUser();
            var data = SerializeRegisterData(reqParams);
            MemoryStream mStream = new MemoryStream(ASCIIEncoding.Default.GetBytes(data), true);
            byte[] req = mStream.ToArray();
            try
            {
                var request = WebRequest.Create(url);
                request.ContentType = "application/json; charset=utf-8";
                request.Method = "POST";
                request.ContentLength = req.Length;

                using (Stream os = request.GetRequestStream())
                {
                    os.Write(req, 0, Convert.ToInt32(req.Length));
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
            catch
            {
                return Encoding.ASCII.GetBytes("false");
            }
        }

        protected bool CreateRegistrationFile(byte[] data, string filePath)
        {
            FileStream fileStream = null;

            try
            {
                // Open file for reading
                fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write);

                // Writes a block of bytes to this stream using data from a byte array.
                fileStream.Write(data, 0, data.Length);

                return true;
            }
            catch (Exception ex)
            {
                // Error
                Console.WriteLine("Exception caught: {0}", ex);
            }
            finally
            {
                // close file stream
                if (fileStream != null)
                    fileStream.Close();
            }

            // error occured, return false
            return false;
        }

        protected byte[] GetFromRegistrationFile(string filePath)
        {
            if (!File.Exists(filePath))
                throw new Exception("Registration file is not found");

            FileStream fileStream = null;
            try
            {
                fileStream = File.OpenRead(filePath);
                byte[] bytes = new byte[fileStream.Length];
                fileStream.Read(bytes, 0, Convert.ToInt32(fileStream.Length));
                return bytes;
            }
            finally
            {
                if (fileStream != null)
                    fileStream.Close();
            }
        }

        #endregion

        public bool StartRegistration(out Registration.RegisterData registeredData)
        {
            try
            {
                var driver = this.usbProvider.ActiveUsb;
                if (driver == null)
                    throw new RegistrationException();

                return OnStartRegistration(driver.Name, out registeredData);
            }
            catch
            {
                throw;
            }
        }

        public bool FinalizeRegistration(out Registration.RegisterData registeredData)
        {
            try
            {
                var driver = this.usbProvider.ActiveUsb;
                if (driver == null)
                    throw new RegistrationException();

                return OnFinalizeRegistration(driver.Name, out registeredData);
            }
            catch
            {
                throw;
            }
        }


        public IList<Errors.SynchronizationException> CheckIssues()
        {
            IList<SynchronizationException> errors = null;

            var drive = this.usbProvider.ActiveUsb;
            if (drive == null)
            {
                errors = new List<SynchronizationException>();

                if (this.usbProvider.IsAnyAvailable)
                    errors.Add(new UsbNotChoozenException());
                else
                    errors.Add(new UsbNotPluggedException());
            }


            try
            {
                if (this.requestProcessor.Process<string>(this.urlUtils.GetDefaultUrl(), "False") == "False")
                    throw new LocalHosUnreachableException(); // there is no connection to local host

                /*var netEndPoint = this.urlUtils.GetEnpointUrl();

                // test if there is connection to synchronization endpoint
                if (this.requestProcessor.Process<string>(netEndPoint, "False") == "False")
                    throw new NetUnreachableException(netEndPoint);*/
            }
            catch (Exception ex)
            {
                errors.Add(new NetUnreachableException(ex.Message));
            }

            return errors;
        }
    }
}
