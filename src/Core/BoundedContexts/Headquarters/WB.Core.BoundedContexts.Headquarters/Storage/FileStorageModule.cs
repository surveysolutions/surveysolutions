using System;
using System.Threading.Tasks;
using Amazon.S3;
using Amazon.S3.Transfer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using WB.Core.BoundedContexts.Headquarters.Storage.AmazonS3;
using WB.Core.GenericSubdomains.Portable.ServiceLocation;
using WB.Core.Infrastructure.Modularity;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Implementation.Repositories;
using WB.Core.SharedKernels.DataCollection.Repositories;


namespace WB.Core.BoundedContexts.Headquarters.Storage
{
    public class FileStorageModule : IModule
    {
        public static void Setup(IServiceCollection services, IConfiguration configuration)
        {
            services.AddDefaultAWSOptions(configuration.GetAWSOptions());
            services.AddAWSService<IAmazonS3>();
        }

        public void Load(IIocRegistry registry)
        {
            registry.Bind<IAudioFileStorage, AudioFileStorage>();

            registry.Bind<IAmazonS3Configuration, AmazonS3Configuration>();
            StorageProviderType StorageType(IModuleContext c)
            {
                var storageConfig = c.Get<IOptions<FileStorageConfig>>().Value;
                return storageConfig.GetStorageProviderType();
            }

            registry.Bind<AmazonS3ExternalFileStorage>();
            registry.Bind<NoExternalFileStorage>();
            registry.BindToMethod<IExternalFileStorage>(c => StorageType(c) switch
            {
                StorageProviderType.FileSystem => c.Get<NoExternalFileStorage>(),
                StorageProviderType.AmazonS3 => c.Get<AmazonS3ExternalFileStorage>(),
                _ => throw new ArgumentOutOfRangeException()
            });

            registry.Bind<S3ImageFileStorage>();
            registry.Bind<ImageFileStorage>();
            registry.BindToMethod<IImageFileStorage>(c => StorageType(c) switch
            {
                StorageProviderType.AmazonS3 => c.Get<S3ImageFileStorage>(),
                StorageProviderType.FileSystem => c.Get<ImageFileStorage>(),
            _ => throw new ArgumentOutOfRangeException()
            });

            registry.Bind<AudioAuditFileS3Storage>();
            registry.Bind<AudioAuditFileStorage>();
            registry.BindToMethod<IAudioAuditFileStorage>(c => StorageType(c) switch
            {
                StorageProviderType.AmazonS3 => c.Get<AudioAuditFileS3Storage>(),
                StorageProviderType.FileSystem => c.Get<AudioAuditFileStorage>(),
                _ => throw new ArgumentOutOfRangeException()
            });

            registry.BindToMethod<ITransferUtility>(c => new TransferUtility(c.Get<IAmazonS3>()));
        }

        public Task Init(IServiceLocator serviceLocator, UnderConstructionInfo status)
        {
            return Task.CompletedTask;
        }
    }

    public static class StorageExtensions
    {
    }
}
