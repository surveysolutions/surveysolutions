﻿using System;
using System.IO;
using Amazon;
using Amazon.Extensions.NETCore.Setup;
using Amazon.S3;
using Amazon.S3.Transfer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using WB.Core.BoundedContexts.Headquarters.Implementation.Repositories;
using WB.Core.BoundedContexts.Headquarters.Storage.AmazonS3;
using WB.Core.Infrastructure.Modularity;
using WB.Core.SharedKernels.Configs;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Implementation.Repositories;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Infrastructure.Native.Workspaces;


namespace WB.Core.BoundedContexts.Headquarters.Storage
{
    public class FileStorageModule : IModule
    {
        private static readonly object lockObject = new object();

        public static void Setup(IServiceCollection services, IConfiguration configuration)
        {
            AWSConfigsS3.UseSignatureVersion4 = true;
            var awsOptions = configuration.GetAWSOptions();
            services.AddDefaultAWSOptions(awsOptions);

            services.AddTransient<IAmazonS3>(s =>
            {
                var client = awsOptions.CreateServiceClient<IAmazonS3>();
                if(client.Config is AmazonS3Config s3)
                {
                    s3.ForcePathStyle = configuration.GetSection("AWS").GetValue("ForcePathStyle", false);
                }
                return client;
            });
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

            registry.BindToMethod<IOptions<FileStorageConfig>>(sp =>
            {
                var configuration = sp.Get<IConfiguration>();
                var logger = sp.Get<ILogger<FileStorageModule>>();
                var workspaceAccessor = sp.Get<IWorkspaceContextAccessor>();

                var fileStorageConfig = configuration.GetSection("FileStorage").Get<FileStorageConfig>();
                fileStorageConfig.AppData = fileStorageConfig.AppData.Replace("~", Directory.GetCurrentDirectory());
                fileStorageConfig.TempData = fileStorageConfig.TempData.Replace("~", Directory.GetCurrentDirectory());
                fileStorageConfig.GlobalTempData = fileStorageConfig.TempData;

                var workspace = workspaceAccessor.CurrentWorkspace();

                if (workspace != null && workspace.Name != WorkspaceConstants.DefaultWorkspaceName)
                {
                    // only append workspace name to FileSystem.
                    // AmazonS3Configuration will handle workspaces prefixing
                    // fs and s3 has different scope model
                    // fs: <data_site>/<workspace?>/data
                    // s3: <bucketName>/hq/<tenant>/<workspace?>/data
                    if (fileStorageConfig.GetStorageProviderType() == StorageProviderType.FileSystem)
                    {
                        fileStorageConfig.AppData = Path.Combine(fileStorageConfig.AppData, workspace.Name);
                        fileStorageConfig.TempData = Path.Combine(fileStorageConfig.TempData, workspace.Name);
                    }
                }

                void EnsureFolderExists(string folder)
                {
                    if (Directory.Exists(folder)) return;
                    lock (lockObject)
                    {
                        if (Directory.Exists(folder)) return;

                        Directory.CreateDirectory(folder);
                        logger.LogInformation("Created {folder} folder", folder);
                    }
                }

                if (fileStorageConfig.GetStorageProviderType() == StorageProviderType.FileSystem)
                {
                    EnsureFolderExists(fileStorageConfig.AppData);
                }

                EnsureFolderExists(fileStorageConfig.TempData);
                EnsureFolderExists(fileStorageConfig.GlobalTempData);

                return Options.Create(fileStorageConfig);
            });
        }
    }

    public static class StorageExtensions
    {
    }
}
