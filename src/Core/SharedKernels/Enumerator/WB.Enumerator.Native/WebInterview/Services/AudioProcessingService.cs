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
using Microsoft.Extensions.Options;
using WB.Core.SharedKernels.DataCollection;
using WB.Infrastructure.Native.Monitoring;
using Xabe.FFmpeg;

namespace WB.Enumerator.Native.WebInterview.Services
{
    public class AudioProcessingService : IAudioProcessingService
    {
        private readonly ILogger<AudioProcessingService> logger;
        private readonly IOptions<FileStorageConfig> fileStorageConfig;
        const string pathInAppDataForFfmpeg = "FFmpeg";
        private const string MimeType = @"audio/m4a";
        private Task audioProcessor;

        public AudioProcessingService(ILogger<AudioProcessingService> logger,
             IOptions<FileStorageConfig> fileStorageConfig)
        {
            this.logger = logger;
            this.fileStorageConfig = fileStorageConfig;
            audioProcessor = Task.Factory.StartNew(AudioCompressionQueueProcessor);
        }


        public Task<AudioFileInformation> CompressAudioFileAsync(byte[] bytes)
        {
            var tcs = new TaskCompletionSource<AudioFileInformation>();
            audioCompressionQueue.Add((tcs, bytes));
            audioFilesInQueue.Inc();
            return tcs.Task;
        }

        private async Task<AudioFileInformation> CompressData(byte[] audio)
        {
            var encodedFile = 
                Path.ChangeExtension(
                    Path.Combine(this.fileStorageConfig.Value.TempData, pathInAppDataForFfmpeg, "resultAudio"), 
                    ".aac");
            var incomingFile = 
                Path.ChangeExtension(
                    Path.Combine(this.fileStorageConfig.Value.TempData, pathInAppDataForFfmpeg, "incomingAudio"), 
                    ".wav");
            var audioResult = new AudioFileInformation();

            try
            {
                await File.WriteAllBytesAsync(incomingFile, audio).ConfigureAwait(false);

                logger.LogInformation("Running conversion for file {source} into {dest}", incomingFile, encodedFile);
                FFmpeg.SetExecutablesPath(this.fileStorageConfig.Value.FFmpegExecutablePath);

                IMediaInfo audioInfo = await FFmpeg.GetMediaInfo(incomingFile)
                    .ConfigureAwait(false);
                audioResult.Duration = audioInfo.Duration;

                IAudioStream audioStream = audioInfo.AudioStreams.First()
                    .SetCodec(AudioCodec.aac)
                    .SetBitrate(64 * 1024);

                await FFmpeg.Conversions.New()
                    .AddStream(audioStream)
                    .SetOutput(encodedFile)
                    .Start()
                    .ConfigureAwait(false);

                
                audioResult.Binary = await File.ReadAllBytesAsync(encodedFile).ConfigureAwait(false);
                audioResult.MimeType = MimeType;

                logger.LogInformation("Done conversion for file {dest}. Reduced size from {srcSize} to {destSize} (bytes)", 
                    encodedFile, audio.Length, audioResult.Binary.Length);

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
                if(File.Exists(encodedFile))
                    File.Delete(encodedFile);
                if(File.Exists(incomingFile))
                    File.Delete(incomingFile);
            }
        }

        private readonly BlockingCollection<(TaskCompletionSource<AudioFileInformation> task, byte[] bytes)> audioCompressionQueue
            = new BlockingCollection<(TaskCompletionSource<AudioFileInformation>, byte[])>();

        private async Task AudioCompressionQueueProcessor()
        {
            audioFilesInQueue.Set(0);

            Thread.CurrentThread.Name = "Audio compression queue";
            var sw = new Stopwatch();

            foreach (var job in audioCompressionQueue.GetConsumingEnumerable())
            {
                try
                {
                    sw.Restart();
                    
                    var result = await CompressData(job.bytes);

                    string resultHash = CalculateHash(result.Binary);
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
