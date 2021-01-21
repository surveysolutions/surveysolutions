using Amazon;
using Amazon.S3;
using Amazon.S3.Transfer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using WB.Services.Export.Checks;
using WB.Services.Export.Services.Processing;

namespace WB.Services.Export.Storage
{
    public class FileStorageModule
    {
        public static void Register(IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<AmazonS3Settings>(configuration.GetSection("storage:s3"));

            var isS3Enabled = configuration.IsS3Enabled();

            if (isS3Enabled)
            {
                services.AddTransient<IExternalArtifactsStorage, S3ArtifactsStorage>();

                var options = configuration.GetAWSOptions("AWS");
                
                services.AddDefaultAWSOptions(options);

                services.Configure<AmazonS3Settings>(configuration.GetSection("AWS"));
                services.Configure<S3StorageSettings>(configuration.GetSection("Storage:S3"));

                services.AddAWSService<IAmazonS3>();
                services.AddTransient<IAmazonS3>(service =>
                {
                    var s3Settings = service.GetRequiredService<IOptions<AmazonS3Settings>>().Value;
                    
                    return s3Settings.AccessKey != null && s3Settings.SecretKey != null
                        ? new AmazonS3Client(s3Settings.AccessKey, s3Settings.SecretKey, options.Region)
                        : new AmazonS3Client(options.Region ?? RegionEndpoint.USEast1);
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
