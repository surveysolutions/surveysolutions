using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Main.Core;
using Newtonsoft.Json;

namespace WB.Core.Synchronization.SyncStorage
{
    public class FileChunkStorage : IChunkStorage
    {
        private readonly string path;
        private const string folderName = "SyncData";
        private const string fileExtension = "sync";
        private long currentSequence = 1;
        private  object myLock = new object();

        public FileChunkStorage(string folderPath/*, Guid supervisor*/)
        {
            this.path = Path.Combine(folderPath, folderName/*, supervisor.ToString()*/);
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
            var alavibleFiles = GetAllFiles();
            if (alavibleFiles.Any())
                currentSequence = GetAllFiles().Max(x => x.Key) + 1;
        }

       
        public void StoreChunk(Guid id, string syncItem)
        {
            lock (myLock)
            {
                File.WriteAllText(GetFilePath(id, currentSequence), syncItem);
                currentSequence++;
            }
        }

        public string ReadChunk(Guid id)
        {
            var syncDir = new DirectoryInfo(path);
            var sequences =
                syncDir.GetFiles(string.Format("*-{0}.{1}", id, fileExtension))
                       .Select(ExctractSequence)
                       .OrderByDescending(s => s);
            if (!sequences.Any())
                throw new ArgumentException("chunk is absent");

            return File.ReadAllText(GetFilePath(id, sequences.FirstOrDefault()));
        }

        public IEnumerable<Guid> GetChunksCreatedAfter(long sequence)
        {
            var sequences = GetAllFiles().Where(f => f.Key > sequence);
            return sequences.Select(f => f.Value).Distinct().ToList();
        }

        public IEnumerable<KeyValuePair<long, Guid>> GetChunkPairsCreatedAfter(long sequence)
        {
            var sequences = GetAllFiles().Where(f => f.Key > sequence);

            return sequences.GroupBy(i => i.Value)
                .Select(pair => pair.First(x => x.Key == pair.Max(y => y.Key)));
        }

        private string GetFilePath(Guid id, long sequence)
        {
            return Path.Combine(this.path, string.Format("{0}-{1}.{2}", sequence, id, fileExtension));
        }

        private IEnumerable<KeyValuePair<long, Guid>> GetAllFiles()
        {
            var syncDir = new DirectoryInfo(path);

            return
                syncDir.GetFiles(string.Format("*.{0}", fileExtension))
                       .ToDictionary(ExctractSequence, ExctractChuncId);
        }

        private Guid ExctractChuncId(FileInfo f)
        {
            var guidStartsIndex = f.Name.IndexOf('-');
            var guidAsString = f.Name.Substring(guidStartsIndex + 1, f.Name.LastIndexOf('.') - guidStartsIndex - 1);
            return Guid.Parse(guidAsString);
        }

        private long ExctractSequence(FileInfo f)
        {
            var sequenceAsString = f.Name.Substring(0, f.Name.IndexOf('-'));
            return long.Parse(sequenceAsString);
        }
    }
}