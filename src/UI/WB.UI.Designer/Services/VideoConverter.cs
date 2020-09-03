using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using WB.Core.BoundedContexts.Designer.Services;
using WB.UI.Designer.Models;

namespace WB.UI.Designer.Services
{
    public class VideoConverter : IVideoConverter
    {
        private readonly IOptions<IntegrationsConfig> integrations;

        public VideoConverter(IOptions<IntegrationsConfig> integrations)
        {
            this.integrations = integrations;
        }

        public byte[] CreateThumbnail(byte[] videoBytes)
        {
            string tempVideoFile = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            string thumbVideoFile = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());

            File.WriteAllBytes(tempVideoFile, videoBytes);
            
            try
            {
                var pathToFfmpeg = Path.Combine(this.integrations.Value.FFmpegExecutablePath, "ffmpeg");
                
                Infrastructure.Native.Utils.ConsoleCommand.Run(pathToFfmpeg
                    , $"-ss {5} -i {"\"" + tempVideoFile + "\""} -f image2 -vframes 1 -y {"\"" + thumbVideoFile + "\""}");
                return File.ReadAllBytes(thumbVideoFile);
            }
            finally
            {
                if(File.Exists(tempVideoFile)) File.Delete(tempVideoFile);
                if (File.Exists(thumbVideoFile)) File.Delete(thumbVideoFile);
            }
        }
    }
}
