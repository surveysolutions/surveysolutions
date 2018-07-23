using System.Configuration;
using System.Threading.Tasks;
using Amazon.S3;
using Amazon.S3.Transfer;
using WB.Core.BoundedContexts.Headquarters.Storage.AmazonS3;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Portable.ServiceLocation;
using WB.Core.Infrastructure.Modularity;
using WB.Core.SharedKernels.DataCollection.Implementation.Repositories;
using WB.Core.SharedKernels.DataCollection.Repositories;

namespace WB.Core.BoundedContexts.Headquarters.Storage
{
    public class FileStorageModule : IModule
    {
        private readonly string currentFolderPath;

        public FileStorageModule(string currentFolderPath)
        {
            this.currentFolderPath = currentFolderPath;
        }

        public void Load(IIocRegistry registry)
        {
            registry.Bind<IAudioFileStorage, AudioFileStorage>();

            var isS3Enabled = ConfigurationManager.AppSettings["Storage.S3.Enable"].ToBool(true);

            if (isS3Enabled)
            {
                registry.Bind<IExternalFileStorage, S3FileStorage>();

                registry.BindToMethodInSingletonScope(ctx =>
                {
                    var settings = ConfigurationManager.AppSettings;

                    return new AmazonS3Settings
                    {
                        BucketName = settings["Storage.S3.BucketName"],
                        Region = settings["Storage.S3.Region"],
                        Prefix = settings["Storage.S3.Prefix"],
                        Endpoint = settings["Storage.S3.Endpoint"]
                    };
                });

                registry.BindToMethodInSingletonScope<IAmazonS3>(c =>
                {
                    var s3Settings = c.Get<AmazonS3Settings>();
                    return new AmazonS3Client(s3Settings.Config());
                });

                registry.BindToMethod<ITransferUtility>(c => new TransferUtility(c.Get<IAmazonS3>()));

                registry.BindAsSingleton<IImageFileStorage, S3ImageFileStorage>();
            }
            else
            {
                registry.Bind<IExternalFileStorage, NoExternalFileSystemStorage>();

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
