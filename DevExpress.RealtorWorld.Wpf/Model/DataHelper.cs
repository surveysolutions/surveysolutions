using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using DevExpress.RealtorWorld.DataService;
using DevExpress.RealtorWorld.Xpf.Helpers;
using System.Deployment.Application;
using System.Windows;
using DevExpress.Utils.Zip;
namespace DevExpress.RealtorWorld.Xpf.Model {
    public class ReusableStream : IDisposable {
        Stream data;
        AutoResetEvent mutex;

        public ReusableStream() {
            this.mutex = new AutoResetEvent(true);
        }
        public Stream Data {
            get { return data; }
            set {
                data = value;
                if(data != null && !data.CanSeek)
                    data = StreamHelper.CopyToMemoryStream(data);
            }
        }
        public void Reset() {
            this.mutex.WaitOne();
            if(this.data != null)
                this.data.Seek(0, SeekOrigin.Begin);
        }
        public void Dispose() {
            this.mutex.Set();
        }
    }
    public static class DataHelper {
#if CLICKONCE
        static bool unPacked = false;
#endif
        static object dataFilesLock = new object();
        static Dictionary<string, ReusableStream> dataFiles = new Dictionary<string, ReusableStream>();

        public static ReusableStream GetDataFile(string name) {
            ReusableStream stream;
            bool loadData = false;
            lock(dataFilesLock) {
                if(!dataFiles.TryGetValue(name, out stream)) {
                    stream = new ReusableStream();
                    loadData = true;
                    dataFiles.Add(name, stream);
                }
            }
            stream.Reset();
            if(loadData) {
                stream.Data = GetDataFileCore(name);
            }
            return stream;
        }
#if CLICKONCE
        public static void UnpackData() {
            string archivesDirectory = Path.Combine(ApplicationDeployment.CurrentDeployment.DataDirectory, "Data");
            string dataDirectory = Path.Combine(ApplicationDeployment.CurrentDeployment.DataDirectory, "Data");
            string keyPath = Path.Combine(archivesDirectory, "Key");
            FileStream keyStream = null;
            int i = 60;
            for(; --i >= 0; ) {
                try {
                    keyStream = new FileStream(keyPath, FileMode.CreateNew, FileAccess.Write, FileShare.None);
                    break;
                } catch {
                    try {
                        File.Delete(keyPath);
                    } catch {
                        Thread.Sleep(1000);
                    }
                }
            }
            if(i < 0) {
                MessageBox.Show("Unpack data error", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            try {
                string[] archiveNames = new string[] { "SharedData.zip" };
                foreach(string archiveName in archiveNames) {
                    string archivePath = Path.Combine(archivesDirectory, archiveName);
                    if(!File.Exists(archivePath)) continue;
                    using(FileStream archiveStream = new FileStream(archivePath, FileMode.Open, FileAccess.Read)) {
                        ZipFileCollection archiveFiles = ZipArchive.Open(archiveStream);
                        foreach(ZipFile archiveFile in archiveFiles) {
                            string dataFilePath = Path.Combine(dataDirectory, archiveFile.FileName);
                            string dataFileDirectory = Path.GetDirectoryName(dataFilePath);
                            Directory.CreateDirectory(dataFileDirectory);
                            byte[] data = new byte[archiveFile.UncompressedSize];
                            archiveFile.FileDataStream.Read(data, 0, data.Length);
                            try {
                                File.WriteAllBytes(dataFilePath, data);
                            } catch { }
                        }
                    }
                }
            } finally {
                try {
                    keyStream.Close();
                    File.Delete(keyPath);
                } catch { }
            }
        }
#endif
        static Stream GetDataFileCore(string name) {
#if CLICKONCE
            if(!unPacked) {
                UnpackData();
                unPacked = true;
            }
#endif
            string filePath = FilesHelper.FindFile(name, FilesHelper.DataPath);
            FileStream fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
            return fileStream;
        }
    }
}
