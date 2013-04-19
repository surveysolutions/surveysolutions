using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using Ionic.Zip;
using Ionic.Zlib;
using Main.Core.Documents;
using Newtonsoft.Json;

namespace WB.UI.Designer.Utils
{
    public interface IZipUtils
    {
        T UnzipTemplate<T>(HttpRequestBase request, HttpPostedFileBase uploadFile) where T : class;
        byte[] ZipDate(string data);
    }

    public class ZipUtils : IZipUtils
    {
        public T UnzipTemplate<T>(HttpRequestBase request, HttpPostedFileBase uploadFile) where T : class
        {
            if (uploadFile == null && request.Files.Count > 0)
            {
                uploadFile = request.Files[0];
            }

            if (uploadFile == null || uploadFile.ContentLength == 0)
            {
                return null;
            }
            if (!ZipFile.IsZipFile(uploadFile.InputStream, false))
            {
                return DesserializeStream<T>(uploadFile.InputStream);
            }
            uploadFile.InputStream.Position = 0;

            try
            {
                //    var list = new List<string>();

                using (ZipFile zip = ZipFile.Read(uploadFile.InputStream))
                {
                    using (var stream = new MemoryStream())
                    {
                        foreach (ZipEntry e in zip)
                        {
                            e.AlternateEncodingUsage = ZipOption.AsNecessary;
                            
                            e.Extract(stream);
                        }
                        return DesserializeStream<T>(stream);
                    }
                }

                // return list;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public byte[] ZipDate(string data)
        {
            var zip = new ZipFile(Encoding.UTF8);

            zip.CompressionLevel = CompressionLevel.BestCompression;
            zip.AddEntry(
                "data.txt", data,Encoding.UTF8);
            var outputStream = new MemoryStream();
            zip.Save(outputStream);
            outputStream.Seek(0, SeekOrigin.Begin);
            return outputStream.ToArray();
        }

        protected T DesserializeStream<T>(Stream stream)
        {
            var settings = new JsonSerializerSettings {TypeNameHandling = TypeNameHandling.Objects};
            stream.Position = 0;
            StreamReader reader = new StreamReader(stream);
            
            string text = reader.ReadToEnd();
            return JsonConvert.DeserializeObject<T>(text, settings);
        }
    }
}