using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using Newtonsoft.Json;
using Common.Utils;

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

        #endregion

        #region C-tor

        protected RegistrationManager(string inFile, string outFile, IRequesProcessor requestProcessor, IUrlUtils urlUtils)
        {
            InFile = inFile;
            OutFile = outFile;

            this.requestProcessor = requestProcessor;
            this.urlUtils = urlUtils;

            RegisrationId = AcceptRegistrationId();
        }

        #endregion

        #region Helpers

        private Guid GetCurrentUser()
        {
            if (this.currentUser.HasValue)
                return this.currentUser.Value;

            this.currentUser = this.requestProcessor.Process<Guid>(urlUtils.GetCurrentUserGetUrl(), "GET", true, Guid.Empty);

            return this.currentUser.Value;
        }

        #endregion

        #region Properties

        public Guid RegisrationId { get; private set; }
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
                return OnAcceptRegistrationId();
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

        public virtual bool StartRegistration(string folderPath)
        {
            var keyContainerName = ContainerName;

            var dataToFile = Encoding.ASCII.GetBytes(SerializeRegisterData(
                new RegisterData { SecretKey = this.rsaCryptoService.GetPublicKey(keyContainerName).Modulus, RegisterId = RegisrationId })
                );

            return CreateRegistrationFile(dataToFile, folderPath + OutFile);
        }

        public abstract bool FinalizeRegistration(string folderPath);

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

        protected byte[] SendRegistrationRequest(byte[] requestParams)
        {
            string url = RegistrationService;

            try
            {
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
    }
}
