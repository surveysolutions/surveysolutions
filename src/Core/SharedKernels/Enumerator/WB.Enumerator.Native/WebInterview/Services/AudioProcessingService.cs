using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using WB.Core.SharedKernels.DataCollection;
using WB.Infrastructure.Native.Monitoring;

namespace WB.Enumerator.Native.WebInterview.Services
{
    public class AudioProcessingService : IAudioProcessingService
    {
        private readonly ILogger<AudioProcessingService> logger;
        private readonly IOptions<FileStorageConfig> fileStorageConfig;
        const string pathInAppDataForFfmpeg = "FFmpeg";
        private const string MimeType = @"audio/m4a";
        private readonly Task audioProcessor;

        public AudioProcessingService(ILogger<AudioProcessingService> logger,
             IOptions<FileStorageConfig> fileStorageConfig)
        {
            this.logger = logger;
            this.fileStorageConfig = fileStorageConfig;
            audioProcessor = Task.Factory.StartNew(AudioCompressionQueueProcessor, TaskCreationOptions.LongRunning);
            TempFilesFolder = Path.Combine(this.fileStorageConfig.Value.TempData, pathInAppDataForFfmpeg);
            if(!Directory.Exists(TempFilesFolder))
            {
                Directory.CreateDirectory(TempFilesFolder);
            }
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
            var destFile = Path.ChangeExtension(Path.Combine(TempFilesFolder, "resultAudio"), ".aac");
            var sourceFile = Path.ChangeExtension(Path.Combine(TempFilesFolder, "incomingAudio"), ".wav");
            var audioResult = new AudioFileInformation();

            try
            {
                logger.LogInformation("Running conversion for file {source} into {dest}", sourceFile, destFile);
                
                await File.WriteAllBytesAsync(sourceFile, audio).ConfigureAwait(false);
                
                var fullPathForSourceFile = Path.GetFullPath(sourceFile);
                var fullPathForDestFile = Path.GetFullPath(destFile);

                this.logger.LogDebug("Starting audio audio encoder in {ffmpegHome}", 
                    this.fileStorageConfig.Value.FFmpegExecutablePath);

                string pathToFfmpeg = this.fileStorageConfig.Value.FFmpegExecutablePath;

                if (!File.Exists(pathToFfmpeg))
                {
                    pathToFfmpeg = Path.Combine(this.fileStorageConfig.Value.FFmpegExecutablePath, "ffmpeg");                    
                }

                if (!File.Exists(pathToFfmpeg))
                {
                    pathToFfmpeg = Path.Combine(this.fileStorageConfig.Value.FFmpegExecutablePath, "ffmpeg.exe");
                }

                if (!File.Exists(pathToFfmpeg))
                {
                    throw new InvalidOperationException("ffmpeg.exe was not found.");
                }

                var ffmpegOutput = Infrastructure.Native.Utils.ConsoleCommand.Read(pathToFfmpeg
                    , $"-hide_banner -i {fullPathForSourceFile} -y -c:a aac -b:a 64k {fullPathForDestFile}");
                
                var match = Regex.Match(ffmpegOutput, @"Duration: (\d\d):(\d\d):((\d\d)(\.\d\d)?)", 
                    RegexOptions.None,TimeSpan.FromMilliseconds(3000));
                var hours = Int32.Parse(match.Groups[1].Value);
                var minutes = Int32.Parse(match.Groups[2].Value);
                var seconds = Int32.Parse(match.Groups[4].Value);
                int milliseconds = 0;
                if (match.Groups.Count > 4)
                {
                    milliseconds = Int32.Parse(match.Groups[5].Value.Replace(".", ""));
                }

                audioResult.Duration = new TimeSpan(0, hours, minutes, seconds, milliseconds);
                audioResult.Binary = await File.ReadAllBytesAsync(destFile).ConfigureAwait(false);
                audioResult.MimeType = MimeType;

                logger.LogInformation("Done conversion for file {dest}. Reduced size from {srcSize} to {destSize} (bytes)", 
                    destFile, audio.Length, audioResult.Binary.Length);

                return audioResult;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error on compress audio");

                audioResult.MimeType = @"audio/wav";
                audioResult.Binary = audio;
                audioResult.Duration = TimeSpan.FromSeconds(0);

                return audioResult;
            }
            finally
            {
                if(File.Exists(destFile))
                    File.Delete(destFile);
                if(File.Exists(sourceFile))
                    File.Delete(sourceFile);
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
        private readonly string TempFilesFolder;
    }

    public class AudioFileInformation
    {
        public TimeSpan Duration { get; set; }
        public string Hash { get; set; }
        public byte[] Binary { get; set; }
        public string MimeType { get; set; }
    }
}
