using System.Threading.Tasks;
using Amazon.S3;
using Amazon.S3.Transfer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using WB.Core.BoundedContexts.Headquarters.Storage.AmazonS3;
using WB.Core.GenericSubdomains.Portable.ServiceLocation;
using WB.Core.Infrastructure.Modularity;
using WB.Core.SharedKernels.DataCollection.Implementation.Repositories;
using WB.Core.SharedKernels.DataCollection.Repositories;

namespace WB.Core.BoundedContexts.Headquarters.Storage
{
    public class FileStorageModule : IModule
    {
        private readonly IConfiguration configuration;

        public FileStorageModule(IConfiguration configuration)
        {
            this.configuration = configuration;
        }

        public void Load(IIocRegistry registry)
        {
            registry.Bind<IAudioFileStorage, AudioFileStorage>();
            

            var settings = configuration.AmazonOptions().Get<AmazonS3Settings>();

            if (settings.IsEnabled)
            {
                registry.Bind<IExternalFileStorage, S3FileStorage>();

                registry.BindToMethodInSingletonScope<IAmazonS3>(c =>
                {
                    var s3Settings = c.Get<IOptions<AmazonS3Settings>>();
                    return new AmazonS3Client(s3Settings.Value.Config());
                });

                registry.BindToMethod<ITransferUtility>(c => new TransferUtility(c.Get<IAmazonS3>()));

                registry.BindAsSingleton<IImageFileStorage, S3ImageFileStorage>();
                registry.Bind<IAudioAuditFileStorage, AudioAuditFileS3Storage>();
            }
            else
            {
                registry.Bind<IExternalFileStorage, NoExternalFileSystemStorage>();
                registry.Bind<IAudioAuditFileStorage, AudioAuditFileStorage>();
                registry.Bind<IImageFileStorage, ImageFileStorage>();
            }
        }

        public Task Init(IServiceLocator serviceLocator, UnderConstructionInfo status)
        {
            return Task.CompletedTask;
        }
    }

    public static class StorageExtensions
    {
        public static IConfigurationSection AmazonOptions(this IConfiguration configuration) => configuration.GetSection("Amazon");
    }
}
