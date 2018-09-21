using System;
using Amazon.S3;
using Amazon.S3.Transfer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using WB.Services.Export.Services.Processing.Good;
using WB.Services.Export.Services.Storage;

namespace WB.Services.Export.Storage
{
    public class FileStorageModule
    {
        public static void Register(IServiceCollection services, IConfiguration configuration)
        {
            // services.AddTransient<IAudioFileStorage, AudioFileStorage>();
            services.Configure<AmazonS3Settings>(configuration.GetSection("s3"));

            var isS3Enabled = configuration.GetSection("s3").GetValue<bool>("IsEnabled");

            if (isS3Enabled)
            {
                services.AddTransient<IExternalFileStorage, S3FileStorage>();

                services.AddTransient<IAmazonS3>(service =>
                {
                    var s3Settings = service.GetService<IOptions<AmazonS3Settings>>();
                    return new AmazonS3Client(s3Settings.Value.Config());
                });

                services.AddTransient<ITransferUtility>(c => new TransferUtility(c.GetService<IAmazonS3>()));
                services.AddTransient<IImageFileStorage, S3ImageFileStorage>();
            }
            else
            {
                services.AddTransient<IExternalFileStorage, NoExternalFileSystemStorage>();

                
                //registry.BindAsSingletonWithConstructorArgument<IImageFileStorage, ImageFileStorage>(
                //    "rootDirectoryPath", this.currentFolderPath);
            }
        }
    }
}
