using System;
using System.Collections.Generic;
using System.IO;
using Ionic.Zip;

namespace WB.Core.SharedKernels.SurveyManagement.Web.Utils.Compression
{
    public static class ZipManager
    {
        public static List<string> GetZipContent(Stream inputStream)
        {
            if (!ZipFile.IsZipFile(inputStream, false))
            {
                return null;
            }

            inputStream.Position = 0;

            try
            {
                var list = new List<string>();

                using (ZipFile zip = ZipFile.Read(inputStream))
                {
                    using (var stream = new MemoryStream())
                    {
                        foreach (ZipEntry e in zip)
                        {
                            e.Extract(stream);
                        }
                        stream.Position = 0;
                        using (StreamReader reader = new StreamReader(stream))
                        {
                            string text = reader.ReadToEnd();
                            list.Add(text);
                        }
                    }
                }

                return list;
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}