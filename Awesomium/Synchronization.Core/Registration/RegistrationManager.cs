using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using Newtonsoft.Json;


namespace Synchronization.Core.Registration
{
    public abstract class RegistrationManager
    {
        protected string SerializeRegisterData(RegisterData data)
        {
            var settings = new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.Objects };
            return JsonConvert.SerializeObject(data, Formatting.Indented, settings);
        }

        protected byte[] SendPostWebRequest(string url, string requestParams)
        {
            byte[] buffer = new byte[4097];
            byte[] result = null;


            WebRequest request = WebRequest.Create(url);
            request.ContentType = "application/x-www-form-urlencoded";
            request.Method = "POST";
            byte[] bytes = Encoding.ASCII.GetBytes(requestParams);
            request.ContentLength = bytes.Length;
            Stream os = request.GetRequestStream();
            os.Write(bytes, 0, bytes.Length); //Push it out there
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
            }
            while (true);

            result = memoryStream.ToArray();
            // streamReader.Close();
            // Let the parent thread know the process is done

            responseStream.Close();
            memoryStream.Close();
            return result;


        }
        protected bool FormRegistrationFile(byte[] data, string filePath)
        {
            try
            {
                // Open file for reading
                FileStream _FileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write);

                // Writes a block of bytes to this stream using data from a byte array.
                _FileStream.Write(data, 0, data.Length);

                // close file stream
                _FileStream.Close();

                return true;
            }
            catch (Exception _Exception)
            {
                // Error
                Console.WriteLine("Exception caught in process: {0}", _Exception.ToString());
            }

            // error occured, return false
            return false;
        }

        protected byte[] GetFromRegistrationFile(string filePath)
        {
            FileStream fs = File.OpenRead(filePath);
            try
            {
                byte[] bytes = new byte[fs.Length];
                fs.Read(bytes, 0, Convert.ToInt32(fs.Length));
                fs.Close();
                return bytes;
            }
            finally
            {
                fs.Close();
            }
        }

        public abstract void RegistrationFirstStep(IRSACryptoService rsaCryptoService);

        public abstract void RegistrationFirstStep(IRSACryptoService rsaCryptoService, string user);

        public abstract void RegistrationSecondStep();
    }
}
