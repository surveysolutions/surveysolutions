using System;
using System.IO;
using System.Net;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Newtonsoft.Json;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Portable.Implementation;

namespace WB.Core.Infrastructure.FileSystem
{
    public class FileSystemAccessorBase
    {
        public string CombinePath(string path1, string path2) => Path.Combine(path1, path2);
        public string CombinePath(params string[] pathes) => Path.Combine(pathes);
        public string GetFileName(string filePath) => Path.GetFileName(filePath);
        public string GetFileNameWithoutExtension(string filePath) => Path.GetFileNameWithoutExtension(filePath);
        public string GetFileExtension(string filePath) => Path.GetExtension(filePath);

        public void WriteAllText(string pathToFile, string content) => File.WriteAllText(pathToFile, content);

        public void WriteAllBytes(string pathToFile, byte[] content)
        {
            File.WriteAllBytes(pathToFile, content);
        }
        
        public Task WriteAllBytesAsync(string pathToFile, byte[] content)
        {
            return File.WriteAllBytesAsync(pathToFile, content);
        }

        public bool IsHashValid(byte[] fileContent, byte[] hash)
        {
            using (var crypto = MD5.Create())
            {
                var contentHash = crypto.ComputeHash(fileContent);

                return contentHash.SequenceEqual(hash);
            }
        }

        public byte[] ReadHash(Stream stream)
        {
            using var crypto = MD5.Create();
            var hash = crypto.ComputeHash(stream);
            return hash;
        }
        
        public byte[] ReadHash(string pathToFile)
        {
            if (!File.Exists(pathToFile))
            {
                return null;
            }

            lock (hashLock)
            {
                var hashFile = pathToFile + ".md5";
                var fileInfo = new FileInfo(pathToFile);
                var lastWrite = new DateTimeOffset(fileInfo.LastWriteTimeUtc).ToUnixTimeMilliseconds();

                if (File.Exists(hashFile))
                {
                    var hash = ReadHashFile();
                    if (hash?.LastWriteTime != lastWrite)
                    {
                        File.Delete(hashFile);
                    }
                }

                FileHash fileHash;

                if (!File.Exists(hashFile))
                {
                    lock (hashLock)
                    {
                        if (!File.Exists(hashFile))
                        {
                            using var crypto = MD5.Create();
                            var hash = crypto.ComputeHash(File.ReadAllBytes(pathToFile));

                            fileHash = new FileHash
                            {
                                MD5 = hash,
                                LastWriteTime = lastWrite
                            };

                            var json = JsonConvert.SerializeObject(fileHash);
                            File.WriteAllText(hashFile, json);

                            return hash;
                        }
                    }
                }

                fileHash = ReadHashFile();

                return fileHash.MD5;

                FileHash ReadHashFile()
                {
                    try
                    {
                        return JsonConvert.DeserializeObject<FileHash>(File.ReadAllText(hashFile));
                    }
                    catch
                    {
                        return null;
                    }
                }
            }
        }

        private class FileHash
        {
            public long LastWriteTime { get; set; }
            public byte[] MD5 { get; set; }
        }

        static readonly object hashLock = new object();

        public byte[] ReadAllBytes(string pathToFile, long? start = null, long? length = null)
        {
            if (start != null)
            {
                using (var reader = File.OpenRead(pathToFile))
                {
                    return reader.ReadExactly(start.Value, length);
                }
            }
            else return File.ReadAllBytes(pathToFile);
        }

        public string ReadAllText(string pathToFile) => File.ReadAllText(pathToFile);
    }
}
