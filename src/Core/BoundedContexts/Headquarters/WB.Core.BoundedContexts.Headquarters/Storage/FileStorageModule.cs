using System.Threading.Tasks;
using Amazon.S3;
using Amazon.S3.Transfer;
using WB.Core.BoundedContexts.Headquarters.Storage.AmazonS3;
using WB.Core.GenericSubdomains.Portable.ServiceLocation;
using WB.Core.Infrastructure.Modularity;
using WB.Core.SharedKernels.DataCollection.Implementation.Repositories;
using WB.Core.SharedKernels.DataCollection.Repositories;

namespace WB.Core.BoundedContexts.Headquarters.Storage
{
    public class FileStorageModule : IModule
    {
        private readonly string currentFolderPath;
        private readonly bool isS3Enabled;
        private readonly string bucketName;
        private readonly string region;
        private readonly string prefix;
        private readonly string endpoint;

        public FileStorageModule(string currentFolderPath,
            bool isS3enabled,
            string bucketName,
            string region,
            string prefix, 
            string endpoint)
        {
            this.currentFolderPath = currentFolderPath;
            this.isS3Enabled = isS3enabled;
            this.bucketName = bucketName;
            this.region = region;
            this.prefix = prefix;
            this.endpoint = endpoint;
        }

        public void Load(IIocRegistry registry)
        {
            registry.Bind<IAudioFileStorage, AudioFileStorage>();
            
            if (isS3Enabled)
            {
                registry.Bind<IExternalFileStorage, S3FileStorage>();

                registry.BindToMethodInSingletonScope(ctx =>
                {

                    return new AmazonS3Settings
                    {
                        BucketName = bucketName,
                        Region = region, 
                        Prefix = prefix, 
                        Endpoint = endpoint 
                    };
                });

                registry.BindToMethodInSingletonScope<IAmazonS3>(c =>
                {
                    var s3Settings = c.Get<AmazonS3Settings>();
                    return new AmazonS3Client(s3Settings.Config());
                });

                registry.BindToMethod<ITransferUtility>(c => new TransferUtility(c.Get<IAmazonS3>()));

                registry.BindAsSingleton<IImageFileStorage, S3ImageFileStorage>();
                registry.Bind<IAudioAuditFileStorage, AudioAuditFileS3Storage>();
            }
            else
            {
                registry.Bind<IExternalFileStorage, NoExternalFileSystemStorage>();
                registry.Bind<IAudioAuditFileStorage, AudioAuditFileStorage>();

                registry.BindAsSingletonWithConstructorArgument<IImageFileStorage, ImageFileStorage>(
                    "rootDirectoryPath", this.currentFolderPath);
            }
        }

        public Task Init(IServiceLocator serviceLocator, UnderConstructionInfo status)
        {
            return Task.CompletedTask;
        }
    }
}
