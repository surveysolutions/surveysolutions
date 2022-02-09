using System;
using System.IO;
using System.IO.Compression;
using System.Text;
using System.Xml;
using WB.Core.BoundedContexts.Headquarters.Services;

namespace WB.Core.BoundedContexts.Headquarters.Implementation.Services
{
    internal class AndroidPackageReader : IAndroidPackageReader
    {
        private const string VersionNameAttributeName = "versionName";
        private const string VersionCodeAttributeName = "versionCode";

        public AndroidPackageInfo Read(Stream fileStream)
        {
            byte[] manifestData = null;

            using (ZipArchive zip = new ZipArchive(fileStream))
            {
                var allFound = 0;
                foreach (var zipArchiveEntry in zip.Entries)
                {
                    if (zipArchiveEntry.Name.ToLower() == "androidmanifest.xml")
                    {
                        manifestData = new byte[50 * 1024];
                        using (Stream strm = zipArchiveEntry.Open())
                        {
                            strm.Read(manifestData, 0, manifestData.Length);
                        }
                        allFound++;
                    }

                    if (allFound == 1)
                        break;
                }
            }

            string xml = new APKManifest().ReadManifestFileIntoXml(manifestData);
            XmlDocument manifestXml = new XmlDocument();
            manifestXml.LoadXml(xml);
            var versionCode = this.ExtractValueByAttributeNameFromManifest(manifestXml, VersionCodeAttributeName);
            var versionString = this.ExtractValueByAttributeNameFromManifest(manifestXml, VersionNameAttributeName);

            return new AndroidPackageInfo
            {
                BuildNumber = string.IsNullOrEmpty(versionCode) ? (int?)null : int.Parse(versionCode),
                VersionString = versionString
            };
        }
        
        public AndroidPackageInfo Read(string pathToApkFile)
        {
            using (var fileStream = File.OpenRead(pathToApkFile))
            {
                return Read(fileStream);
            }
        }
        
        private class APKManifest
        {
            private string result = "";
            private static int startDocTag = 1048832;
            private static int endDocTag = 1048833;
            private static int startTag = 1048834;
            private static int endTag = 1048835;
            private static int textTag = 1048836;
            private static string spaces = "                                             ";

            public string ReadManifestFileIntoXml(byte[] manifestFileData)
            {
                bool flag = manifestFileData.Length == 0;
                if (flag)
                {
                    throw new Exception("Failed to read manifest data.  Byte array was empty");
                }
                int num = this.LEW(manifestFileData, 16);
                int num2 = 36;
                int stOff = num2 + num * 4;
                int num3 = this.LEW(manifestFileData, 12);
                for (int i = num3; i < manifestFileData.Length - 4; i += 4)
                {
                    bool flag2 = this.LEW(manifestFileData, i) == APKManifest.startTag;
                    if (flag2)
                    {
                        num3 = i;
                        break;
                    }
                }
                int j = num3;
                int num4 = 0;
                int num5 = 1;
                while (j < manifestFileData.Length)
                {
                    int num6 = this.LEW(manifestFileData, j);
                    int num7 = this.LEW(manifestFileData, j + 8);
                    int num8 = this.LEW(manifestFileData, j + 16);
                    int strInd = this.LEW(manifestFileData, j + 20);
                    bool flag3 = num6 == APKManifest.startTag;
                    if (flag3)
                    {
                        int num9 = this.LEW(manifestFileData, j + 24);
                        int num10 = this.LEW(manifestFileData, j + 28);
                        j += 36;
                        string str = this.compXmlString(manifestFileData, num2, stOff, strInd);
                        string text = "";
                        int num15;
                        for (int k = 0; k < num10; k = num15 + 1)
                        {
                            int num11 = this.LEW(manifestFileData, j);
                            int strInd2 = this.LEW(manifestFileData, j + 4);
                            int num12 = this.LEW(manifestFileData, j + 8);
                            int num13 = this.LEW(manifestFileData, j + 12);
                            int num14 = this.LEW(manifestFileData, j + 16);
                            j += 20;
                            string text2 = this.compXmlString(manifestFileData, num2, stOff, strInd2);
                            string text3 = (num12 != -1) ? this.compXmlString(manifestFileData, num2, stOff, num12) : num14.ToString();
                            text = string.Concat(new string[]
                            {
                            text,
                            " ",
                            text2,
                            "=\"",
                            text3,
                            "\""
                            });
                            num15 = k;
                        }
                        this.prtIndent(num4, "<" + str + text + ">");
                        num15 = num4;
                        num4 = num15 + 1;
                    }
                    else
                    {
                        bool flag4 = num6 == APKManifest.endTag;
                        if (flag4)
                        {
                            int num15 = num4;
                            num4 = num15 - 1;
                            j += 24;
                            string str2 = this.compXmlString(manifestFileData, num2, stOff, strInd);
                            this.prtIndent(num4, "</" + str2 + ">  \r\n");
                        }
                        else
                        {
                            bool flag5 = num6 == APKManifest.startDocTag;
                            if (flag5)
                            {
                                int num15 = num5;
                                num5 = num15 + 1;
                                j += 4;
                            }
                            else
                            {
                                bool flag6 = num6 == APKManifest.endDocTag;
                                if (flag6)
                                {
                                    int num15 = num5;
                                    num5 = num15 - 1;
                                    bool flag7 = num5 == 0;
                                    if (flag7)
                                    {
                                        break;
                                    }
                                }
                                else
                                {
                                    bool flag8 = num6 == APKManifest.textTag;
                                    if (!flag8)
                                    {
                                        this.prt(string.Concat(new object[]
                                        {
                                        "  Unrecognized tag code '",
                                        num6.ToString("X"),
                                        "' at offset ",
                                        j
                                        }));
                                        break;
                                    }
                                    uint num16 = 4294967295u;
                                    while (j < manifestFileData.Length)
                                    {
                                        uint num17 = (uint)this.LEW(manifestFileData, j);
                                        j += 4;
                                        bool flag9 = j > manifestFileData.Length;
                                        if (flag9)
                                        {
                                            throw new Exception("Sentinal not found before end of file");
                                        }
                                        bool flag10 = num17 == num16 && num16 == 4294967295u;
                                        if (flag10)
                                        {
                                            num16 = 0u;
                                        }
                                        else
                                        {
                                            bool flag11 = num17 == num16;
                                            if (flag11)
                                            {
                                                break;
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                return this.result;
            }

            private string compXmlString(byte[] xml, int sitOff, int stOff, int strInd)
            {
                bool flag = strInd < 0;
                string text;
                if (flag)
                {
                    text = null;
                }
                else
                {
                    int strOff = stOff + this.LEW(xml, sitOff + strInd * 4);
                    text = this.compXmlStringAt(xml, strOff);
                }
                return text;
            }

            private void prtIndent(int indent, string str)
            {
                this.prt(APKManifest.spaces.Substring(0, Math.Min(indent * 2, APKManifest.spaces.Length)) + str);
            }

            private void prt(string p)
            {
                this.result += p;
            }

            private string compXmlStringAt(byte[] arr, int strOff)
            {
                int num = ((int)arr[strOff + 1] << 8 & 65280) | (int)(arr[strOff] & 255);
                byte[] array = new byte[num];
                int num2;
                for (int i = 0; i < num; i = num2 + 1)
                {
                    array[i] = arr[strOff + 2 + i * 2];
                    num2 = i;
                }
                return Encoding.UTF8.GetString(array);
            }

            private int LEW(byte[] arr, int off)
            {
                return ((int)arr[off + 3] << 24 & -16777216) | ((int)arr[off + 2] << 16 & 16711680) | ((int)arr[off + 1] << 8 & 65280) | (int)(arr[off] & 255);
            }
        }

        private string FindInDocument(XmlDocument doc, string keyName, string attribName)
        {
            XmlNodeList elementsByTagName = doc.GetElementsByTagName(keyName);
            if (elementsByTagName == null) return null;
            
            int num;
            for (int i = 0; i < elementsByTagName.Count; i = num + 1)
            {
                XmlNode xmlNode = elementsByTagName.Item(i);
                bool flag2 = xmlNode.NodeType == XmlNodeType.Element;
                if (flag2)
                {
                    XmlNode namedItem = xmlNode.Attributes.GetNamedItem(attribName);
                    bool flag3 = namedItem != null;
                    if (flag3)
                    {
                        return namedItem.Value;
                    }
                }
                num = i;
            }
            return null;
        }

        private string FuzzFindInDocument(XmlDocument doc, string tag, string attr)
        {
            string[] tAGS = new string[]
            {
                "manifest",
                "application",
                "activity"
            };
            for (int i = 0; i < tAGS.Length; i++)
            {
                string name = tAGS[i];
                XmlNodeList elementsByTagName = doc.GetElementsByTagName(name);
                int num;
                for (int j = 0; j < elementsByTagName.Count; j = num + 1)
                {
                    XmlNode xmlNode = elementsByTagName.Item(j);
                    bool flag = xmlNode.NodeType == XmlNodeType.Element;
                    if (flag)
                    {
                        XmlAttributeCollection attributes = xmlNode.Attributes;
                        for (int k = 0; k < attributes.Count; k = num + 1)
                        {
                            XmlNode xmlNode2 = attributes.Item(k);
                            bool flag2 = xmlNode2.Name.EndsWith(attr);
                            if (flag2)
                            {
                                return xmlNode2.Value;
                            }
                            num = k;
                        }
                    }
                    num = j;
                }
            }
            return null;
        }
        
        public string ExtractValueByAttributeNameFromManifest(XmlDocument manifestXml, string attributeName)
        {
            XmlDocument doc = manifestXml;
            if (doc == null)
                throw new Exception("Document initialize failed");
                
            var versionCode = 
                 this.FindInDocument(doc, "manifest", attributeName) 
              ?? this.FuzzFindInDocument(doc, "manifest", attributeName);
            
            return versionCode;
        }
    }
}
