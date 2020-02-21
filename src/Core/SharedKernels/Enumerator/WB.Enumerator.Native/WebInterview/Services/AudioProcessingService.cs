using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using NAudio.MediaFoundation;
using NAudio.Wave;
using WB.Infrastructure.Native.Monitoring;

namespace WB.Enumerator.Native.WebInterview.Services
{
    public class AudioProcessingService : IAudioProcessingService
    {
        private readonly ILogger<AudioProcessingService> logger;
        private readonly bool audioEncoderStarted = false;

        private const string MimeType = @"audio/m4a";

        public AudioProcessingService(ILogger<AudioProcessingService> logger)
        {
            this.logger = logger;
            try
            {
                MediaFoundationApi.Startup();
                audioEncoderStarted = true;
            }
            catch (Exception e)
            {
                this.logger.LogWarning(e, "Failed to start audio encoder. Web interview audio questions will not be ");
            }
            finally
            {
                // single thread to process all audio compression requests
                // if there is need to process audio in more then one queue - duplicate line below

                Task.Factory.StartNew(AudioCompressionQueueProcessor);
            }
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

                    if (!audioEncoderStarted)
                    {
                        return new AudioFileInformation
                        {
                            Binary = audio,
                            Duration = audioResult.Duration,
                            Hash = CalculateHash(audio),
                            MimeType = "audio/wav"
                        };
                    }

                    const int desiredBitRate = 64 * 1024;
                    MediaFoundationEncoder.EncodeToAac(wavFile, tempFile,  desiredBitRate);
                }

                audioResult.Binary = File.ReadAllBytes(tempFile);
                audioResult.MimeType = MimeType;

                return audioResult;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error on compress audio");

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

            foreach (var job in audioCompressionQueue.GetConsumingEnumerable())
            {
                try
                {
                    sw.Restart();
                    var result = CompressData(job.bytes);

                    var resultHash = CalculateHash(result.Binary);
                    result.Hash = resultHash;
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

        private static string CalculateHash(byte[] data)
        {
            StringBuilder result = new StringBuilder();
            var hashBytes = SHA1.Create().ComputeHash(data);

            foreach (var @byte in hashBytes.Take(4))
            {
                result.Append(@byte.ToString(@"x2"));
            }

            var resultHash = result.ToString();
            return resultHash;
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
