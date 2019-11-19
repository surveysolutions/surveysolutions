using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using NAudio.MediaFoundation;
using NAudio.Wave;
using StackExchange.Exceptional;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Infrastructure.Native.Monitoring;

namespace WB.Enumerator.Native.WebInterview.Services
{
    public class AudioProcessingService : IAudioProcessingService
    {
        private readonly ILogger logger;
        private readonly IHttpContextAccessor httpContextAccessor;

        private const string MimeType = @"audio/m4a";

        public AudioProcessingService(ILogger logger, IHttpContextAccessor httpContextAccessor)
        {
            this.logger = logger;
            this.httpContextAccessor = httpContextAccessor;
            MediaFoundationApi.Startup();
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
            var tempFile = Path.ChangeExtension(Path.GetTempFileName(), ".aac");
            var audioResult = new AudioFileInformation();

            try
            {
                using (var ms = new MemoryStream(audio))
                using (var wavFile = new WaveFileReader(ms))
                {
                    audioResult.Duration = wavFile.TotalTime;

                    const int desiredBitRate = 64 * 1024;
                    MediaFoundationEncoder.EncodeToAac(wavFile, tempFile,  desiredBitRate);
                }

                audioResult.Binary = File.ReadAllBytes(tempFile);
                audioResult.MimeType = MimeType;

                return audioResult;
            }
            catch (Exception ex)
            {
                logger.Error("Error on compress audio", ex);
                ex.Log(httpContextAccessor.HttpContext);

                audioResult.MimeType = @"audio/wav";
                audioResult.Binary = audio;

                return audioResult;
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
            audioFilesInQueue.Set(0);

            Thread.CurrentThread.Name = "Audio compression queue";
            var sw = new Stopwatch();
            var hashBuilder = new StringBuilder();

            foreach (var job in audioCompressionQueue.GetConsumingEnumerable())
            {
                try
                {
                    sw.Restart();
                    var result = CompressData(job.bytes);

                    var hashBytes = SHA1.Create().ComputeHash(result.Binary);
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
        private readonly Gauge audioFilesInQueue =new Gauge(@"wb_audio_queue_files_count", @"Number of audio files in queue");
        private readonly Counter audioFilesProcessed = new Counter(@"wb_audio_files_total", @"Total count of processed audio files");
        private readonly Counter audtioFilesProcessingTime = new Counter(@"wb_audio_files_processing_seconds", @"Total processing time");
    }

    public class AudioFileInformation
    {
        public TimeSpan Duration { get; set; }
        public string Hash { get; set; }
        public byte[] Binary { get; set; }
        public string MimeType { get; set; }
    }
}
