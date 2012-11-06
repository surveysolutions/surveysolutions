using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using Newtonsoft.Json;


namespace Synchronization.Core.Registration
{
    /// <summary>
    /// Base class responsible for registraction of CAPI device on Supervisor
    /// </summary>
    public abstract class RegistrationManager
    {
        #region Members

        private IRSACryptoService rsaCryptoService = new RSACryptoService();

        #endregion

        #region C-tor

        protected RegistrationManager(string inFile, string outFile)
        {
            InFile = inFile;
            OutFile = outFile;

            Id = AcceptId();
        }

        #endregion

        #region Properties

        public Guid Id { get; private set; }
        protected string InFile { get; private set; }
        protected string OutFile { get; private set; }

        #endregion

        #region Helpers

        private Guid AcceptId()
        {
            try
            {
                return OnAcceptId();
            }
            catch // todo: log
            {
                throw;
            }
        }

        #endregion

        #region Abstract Methods

        protected abstract Guid OnAcceptId(); // todo: find out the way to initialize this value properly

        public virtual bool StartRegistration(string folderPath, string keyContainerName = null, string url = null)
        {
            var dataToFile = Encoding.ASCII.GetBytes(SerializeRegisterData(new RegisterData { SecretKey = this.rsaCryptoService.GetPublicKey(keyContainerName).Modulus, TabletId = Id }));

            return CreateRegistrationFile(dataToFile, folderPath + OutFile);
        }

        public abstract bool FinalizeRegistration(string folderPath, string url);

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

        protected byte[] SendRegistrationRequest(string url, byte[] requestParams)
        {
            byte[] buffer = new byte[4097];
            byte[] result = null;

            try
            {
                WebRequest request = WebRequest.Create(url);

                request.ContentType = "application/json; charset=utf-8";
                request.Method = "POST";
                request.ContentLength = requestParams.Length;


                Stream os = request.GetRequestStream();
                os.Write(requestParams, 0, requestParams.Length); //Push it out there
                os.Close();

                WebResponse response = request.GetResponse();

                if (response == null) return null;

                Stream responseStream = response.GetResponseStream();
                MemoryStream memoryStream = new MemoryStream();

                int count = 0;

                do
                {
                    count = responseStream.Read(buffer, 0, buffer.Length);

                    memoryStream.Write(buffer, 0, count);

                    if (count == 0)
                    {
                        break;
                    }
                } while (true);

                result = memoryStream.ToArray();
                // streamReader.Close();
                // Let the parent thread know the process is done

                responseStream.Close();
                memoryStream.Close();
                return result;
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
                Console.WriteLine("Exception caught in process: {0}", ex);
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
