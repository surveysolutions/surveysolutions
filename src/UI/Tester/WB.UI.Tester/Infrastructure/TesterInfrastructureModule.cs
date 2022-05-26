﻿using System.Threading.Tasks;
using WB.Core.BoundedContexts.Tester.Implementation.Services;
using WB.Core.BoundedContexts.Tester.Services;
using WB.Core.BoundedContexts.Tester.ViewModels;
using WB.Core.GenericSubdomains.Portable.Implementation;
using WB.Core.GenericSubdomains.Portable.ServiceLocation;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.HttpServices.HttpClient;
using WB.Core.Infrastructure.HttpServices.Services;
using WB.Core.Infrastructure.Modularity;
using WB.Core.SharedKernels.DataCollection.Implementation.Accessors;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.DataCollection.Utils;
using WB.Core.SharedKernels.Enumerator;
using WB.Core.SharedKernels.Enumerator.Implementation.Services;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure.Storage;
using WB.Core.SharedKernels.Enumerator.Utils;
using WB.UI.Shared.Enumerator.CustomServices;
using WB.UI.Tester.Infrastructure.Internals;
using WB.UI.Tester.Infrastructure.Internals.Rest;
using WB.UI.Tester.Infrastructure.Internals.Security;
using WB.UI.Tester.Infrastructure.Internals.Settings;
using WB.UI.Tester.Infrastructure.Internals.Storage;
using WB.Core.SharedKernels.Questionnaire.Translations;
using WB.UI.Shared.Enumerator.Services;
using WB.UI.Shared.Enumerator.Services.Internals;
using WB.UI.Shared.Enumerator.Services.Logging;
using ILogger = WB.Core.GenericSubdomains.Portable.Services.ILogger;

namespace WB.UI.Tester.Infrastructure
{
    public class TesterInfrastructureModule : IModule
    {
        private readonly string basePath;
        private readonly string questionnaireAssembliesFolder;

        public TesterInfrastructureModule(string basePath, string questionnaireAssembliesFolder = "assemblies")
        {
            this.basePath = basePath;
            this.questionnaireAssembliesFolder = questionnaireAssembliesFolder;
        }

        public void Load(IIocRegistry registry)
        {
            registry.BindAsSingleton<IQuestionnaireImportService, QuestionnaireImportService>();

            registry.Bind<LoginViewModel>();
            registry.Bind<DashboardViewModel>();
            registry.Bind<QuestionnaireDownloadViewModel>();
            registry.Bind<InterviewViewModel>();

            registry.BindToConstant(() => new SqliteSettings
            {
                PathToRootDirectory = AndroidPathUtils.GetPathToInternalDirectory(), 
                PathToDatabaseDirectory = AndroidPathUtils.GetPathToSubfolderInLocalDirectory("data")
            });

            registry.BindAsSingleton(typeof(IPlainStorage<>), typeof(SqlitePlainStorage<>)); // TODO Move to generic module between IN, T

            registry.Bind<ILoggerProvider, NLogLoggerProvider>();
            registry.BindAsSingleton<ILogger, NLogLogger>();
            registry.Bind<IRestServiceSettings, TesterSettings>();
            registry.Bind<INetworkService, AndroidNetworkService>();
            registry.Bind<IEnumeratorSettings, TesterSettings>();
            registry.Bind<IRestServicePointManager, RestServicePointManager>();
            registry.Bind<IHttpClientFactory, AndroidHttpClientFactory>();
            registry.Bind<IRestService, RestService>();
            registry.Bind<IFastBinaryFilesHttpHandler, FastBinaryFilesHttpHandler>();
            registry.Bind<ISerializer, PortableJsonSerializer>();
            registry.Bind<IInterviewAnswerSerializer, NewtonInterviewAnswerJsonSerializer>();
            registry.Bind<IJsonAllTypesSerializer, PortableJsonAllTypesSerializer>();

            registry.Bind<IStringCompressor, JsonCompressor>();
            registry.BindAsSingleton<IDesignerApiService, DesignerApiService>();
            registry.BindAsSingleton<IPrincipal, ITesterPrincipal, TesterPrincipal>();

            registry.BindAsSingletonWithConstructorArgument<IQuestionnaireAssemblyAccessor, TesterQuestionnaireAssemblyAccessor>(
                "assemblyStorageDirectory", AndroidPathUtils.GetPathToSubfolderInLocalDirectory(this.questionnaireAssembliesFolder));

            registry.BindAsSingletonWithConstructorArgument<IAudioFileStorage, TesterAudioFileStorage>("rootDirectoryPath", basePath);
            registry.BindAsSingletonWithConstructorArgument<IImageFileStorage, TesterImageFileStorage>("rootDirectoryPath", basePath);
            registry.Bind<IQuestionnaireTranslator, QuestionnaireTranslator>();
        }
    }
}
