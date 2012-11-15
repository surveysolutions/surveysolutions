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
    public class RegistrationCallbackEventArgs : EventArgs
    {
        public Exception Error { get; private set; }
        public string Message { get; private set; }
        public RegisterData Data { get; private set; }

        public RegistrationCallbackEventArgs(Exception error, string message, RegisterData data)
            : base()
        {
            Error = error;
            Message = message;
            Data = data;
        }

        public bool IsPassed { get { return this.Error == null; } }

        public void AppendMessage(string message)
        {
            if (string.IsNullOrEmpty(Message))
                Message = message;
            else
                Message = Message + message;
        }
    }

    /*public class RegistrationFirstPhaseAccomplished : RegistrationCallbackEventArgs
    {
    }

    public class RegistrationSecondPhaseAccomplished : RegistrationCallbackEventArgs
    {
    }*/

    public delegate void RegistrationCallback(RegistrationManager manager, RegistrationCallbackEventArgs args);

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

        #region Events

        public event RegistrationCallback FirstPhaseAccomplished;
        public event RegistrationCallback SecondPhaseAccomplished;

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

        private byte[] SendRegistrationRequest(byte[] requestParams)
        {
            string url = RegistrationService;

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

        private void CreateRegistrationFile(RegisterData registeredData, string filePath)
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
                // Error
                Console.WriteLine("Exception caught: {0}", ex);

                throw ex;
            }
            finally
            {
                // close file stream
                if (fileStream != null)
                    fileStream.Close();
            }
        }

        private byte[] GetFromRegistrationFile(string filePath)
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

        #region Properties

        public Guid RegistrationId { get { return AcceptRegistrationId(); } }
        public string RegistrationName { get { return AcceptRegistrationName(); } }

        protected string InFile { get; private set; }
        protected string OutFile { get; private set; }
        protected string RegistrationService { get { return this.urlUtils.GetRegistrationCapiPath(); } }

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

        protected abstract Guid OnAcceptRegistrationId();

        protected abstract string OnAcceptRegistrationName();

        protected virtual RegisterData OnStartRegistration(string folderPath)
        {
            var keyContainerName = ContainerName;

            var registeredData = new RegisterData
            {
                SecretKey = this.rsaCryptoService.GetPublicKey(keyContainerName).Modulus,
                RegistrationId = RegistrationId,
                Description = RegistrationName,
                RegisterDate = DateTime.Now,
                Registrator = CurrentUser, // makes no much sence for now, since we do not require CAPI user to be logged on; and on supervisor it coincides with RegistrationId
            };

            CreateRegistrationFile(registeredData, folderPath + OutFile);

            return registeredData;
        }

        protected virtual RegisterData OnFinalizeRegistration(string folderPath)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region Protected Operations

        protected RegisterData AuthorizeAccepetedData(string folderPath)
        {
            try
            {
                var data = GetFromRegistrationFile(folderPath + InFile);

                // assign current user who made registration
                var registerData = DeserializeRegisterData(Encoding.UTF8.GetString(data));
                registerData.Registrator = CurrentUser;

                data = Encoding.UTF8.GetBytes(SerializeRegisterData(registerData));

                var response = SendRegistrationRequest(data);
                var result = Encoding.UTF8.GetString(response, 0, response.Length);

                if (string.Compare(result, "True", true) == 0)
                    return registerData;
                else
                    throw new RegistrationException();
            }
            catch (RegistrationException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new RegistrationException(ex);
            }
        }

        #endregion

        #region Public Methods

        public void StartRegistration()
        {
            Exception error = null;
            RegisterData registeredData = null;

            try
            {
                var driver = this.usbProvider.ActiveUsb;
                if (driver == null)
                    throw new UsbNotChoozenException();

                registeredData = OnStartRegistration(driver.Name);
            }
            catch (Exception e)
            {
                error = e;
            }
            finally
            {
                if (FirstPhaseAccomplished != null)
                    FirstPhaseAccomplished(this, new RegistrationCallbackEventArgs(error, string.Empty, registeredData));
            }
        }

        public void FinalizeRegistration()
        {
            Exception error = null;
            RegisterData registeredData = null;

            try
            {
                var driver = this.usbProvider.ActiveUsb;
                if (driver == null)
                    throw new UsbNotChoozenException();

                registeredData = OnFinalizeRegistration(driver.Name);
            }
            catch (Exception e)
            {
                error = e;
            }
            finally
            {
                if (SecondPhaseAccomplished != null)
                    SecondPhaseAccomplished(this, new RegistrationCallbackEventArgs(error, string.Empty, registeredData));
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
                if (errors == null)
                    errors = new List<SynchronizationException>();

                errors.Add(new NetUnreachableException(ex.Message));
            }

            return errors;
        }

        #endregion
    }
}
