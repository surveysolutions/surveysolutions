using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using Ionic.Zip;
using Main.Core.Documents;
using Newtonsoft.Json;

namespace WB.UI.Designer.Utils
{
    public interface IZipUtils
    {
        T UnzipTemplate<T>(HttpRequestBase request, HttpPostedFileBase uploadFile) where T : class;
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

        protected T DesserializeStream<T>(Stream stream)
        {
            var settings = new JsonSerializerSettings {TypeNameHandling = TypeNameHandling.Objects};
            StreamReader reader = new StreamReader(stream);
            string text = reader.ReadToEnd();
            return JsonConvert.DeserializeObject<T>(text, settings);
        }
    }
}