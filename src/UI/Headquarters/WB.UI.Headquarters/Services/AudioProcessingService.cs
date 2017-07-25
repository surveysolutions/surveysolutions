using System;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;
using CSCore;
using CSCore.Codecs.WAV;
using CSCore.MediaFoundation;
using Prometheus;
using WB.Core.SharedKernels.Questionnaire.Documents.Question;
using System.Diagnostics;
using System.Threading;
using System.Text;

namespace WB.UI.Headquarters.Services
{
    public class AudioProcessingService : IAudioProcessingService
    {
        private const int EncoderBufferSize = 64 * 1024; // just an 64Kb buffer to read
        private const string MimeType = @"audio/m4a";

        public AudioProcessingService()
        {
            // single thread to process all audio compression requests
            // if there is need to process audio in more then one queue - duplicate line below
            Task.Factory.StartNew(AudioCompressionQueueProcessor);
        }

        public Task<AudioFileInformation> CompressAudioFileAsync(byte[] bytes)
        {
            var tcs = new TaskCompletionSource<AudioFileInformation>();
            audioCompressionQueue.Add((tcs, bytes));
            audioFilesInQueue.Inc();
            return tcs.Task;
        }        

        private AudioFileInformation CompressData(byte[] audio)
        {
            var tempFile = Path.GetTempFileName();

            try
            {
                TimeSpan duration;

                using (var ms = new MemoryStream(audio))
                using (var wavFile = new WaveFileReader(ms))
                using (var encoder = MediaFoundationEncoder.CreateAACEncoder(wavFile.WaveFormat, tempFile,
                    (int)AudioQuality.Default * 1024 /* 64 Kb bitrate*/))
                {
                    byte[] buffer = new byte[EncoderBufferSize];

                    long total = 0;
                    int read = 0;
                    wavFile.Position = 0;

                    while ((read = wavFile.Read(buffer, 0, EncoderBufferSize)) > 0)
                    {
                        encoder.Write(buffer, 0, read);
                        total += read;
                    }

                    duration = wavFile.GetLength();
                }

                return new AudioFileInformation
                {
                    Binary = File.ReadAllBytes(tempFile),
                    Duration = duration,
                    MimeType = MimeType
                };
            }
            finally
            {
                File.Delete(tempFile);
            }
        }

        private readonly BlockingCollection<(TaskCompletionSource<AudioFileInformation> task, byte[] bytes)> audioCompressionQueue
            = new BlockingCollection<(TaskCompletionSource<AudioFileInformation>, byte[])>();

        private void AudioCompressionQueueProcessor()
        {
            Thread.CurrentThread.Name = "Audio compression queue";
            var sw = new Stopwatch();
            var sha1 = SHA1.Create();
            var hashBuilder = new StringBuilder();

            foreach (var job in audioCompressionQueue.GetConsumingEnumerable())
            {
                try
                {
                    sw.Restart();
                    var result = CompressData(job.bytes);
                    sha1.Clear();

                    var hashBytes = sha1.ComputeHash(result.Binary);
                    hashBuilder.Clear();

                    foreach (var @byte in hashBytes.Take(4))
                    {
                        hashBuilder.Append(@byte.ToString(@"x2"));
                    }

                    result.Hash = hashBuilder.ToString();
                    job.task.SetResult(result);
                }
                catch (Exception e)
                {
                    job.task.SetException(e);
                }
                finally
                {
                    audioFilesInQueue.Dec();
                    audioFilesProcessed.Inc();
                    audtioFilesProcessingTime.Inc(sw.Elapsed.TotalSeconds);
                }
            }
        }

        // instrumentation
        private readonly Gauge audioFilesInQueue = Metrics.CreateGauge("audio_files_in_queue", @"Number of audio files in queue");
        private readonly Counter audioFilesProcessed = Metrics.CreateCounter(@"audio_files_total", @"Total count of processed audio files");
        private readonly Counter audtioFilesProcessingTime = Metrics.CreateCounter(@"audio_files_processing_seconds", @"Total processing time");
    }

    public class AudioFileInformation
    {
        public TimeSpan Duration { get; set; }
        public string Hash { get; set; }
        public byte[] Binary { get; set; }
        public string MimeType { get; set; }
    }
}