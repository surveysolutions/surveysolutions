using System;
using System.ComponentModel.DataAnnotations;
using Amazon;
using Amazon.Extensions.NETCore.Setup;
using Amazon.S3;
using Amazon.S3.Transfer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Serilog;
using WB.Services.Export.Checks;
using WB.Services.Export.Services.Processing;

namespace WB.Services.Export.Storage
{
    public class FileStorageModule
    {
        public static void Register(IServiceCollection services, IConfiguration configuration)
        {
            var isS3Enabled = configuration.IsS3Enabled();

            if (isS3Enabled)
            {
                AWSConfigsS3.UseSignatureVersion4 = true;
                var awsOptions = configuration.GetAWSOptions();
                services.AddDefaultAWSOptions(awsOptions);
                services.AddTransient<IAmazonS3>(s =>
                {
                    var client = awsOptions.CreateServiceClient<IAmazonS3>();
                    if (client.Config is AmazonS3Config s3)
                    {
                        s3.ForcePathStyle = configuration.GetSection("AWS").GetValue("ForcePathStyle", false);
                    }

                    return client;
                });

                services.AddTransient<IExternalArtifactsStorage, S3ArtifactsStorage>();

                services.Configure<S3StorageSettings>(configuration.GetSection("Storage:S3"));
                services.PostConfigure<S3StorageSettings>(s =>
                {
                    // [Storage:S3].Uri setting will override .BucketName and folder 
                    if (Uri.TryCreate(s.Uri, UriKind.Absolute, out var uri) && uri.Scheme == "s3")
                    {
                        s.BucketName = uri.Host;
                        s.Folder = uri.AbsolutePath.Trim('/');
                    }
                });

                services.AddTransient<ITransferUtility>(c => new TransferUtility(c.GetService<IAmazonS3>()));
                services.AddTransient<IImageFileStorage, S3ImageFileStorage>();
            }
            else
            {
                services.AddTransient<IExternalArtifactsStorage, NoExternalArtifactsSystemStorage>();
            }
        }
    }
}
