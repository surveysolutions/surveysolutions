using System;
using System.Collections.Generic;
using Main.Core.Documents;
using Ncqrs.Eventing.Storage;
using Ninject;
using Ninject.Modules;
using Sqo;
using SQLite.Net.Interop;
using SQLite.Net.Platform.XamarinAndroid;
using WB.Core.BoundedContexts.Interviewer.Implementation.Services;
using WB.Core.BoundedContexts.Interviewer.Services;
using WB.Core.BoundedContexts.Interviewer.Services.Infrastructure;
using WB.Core.BoundedContexts.Interviewer.Views;
using WB.Core.GenericSubdomains.Portable.Implementation;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.DataCollection.Implementation.Accessors;
using WB.Core.SharedKernels.DataCollection.Implementation.Repositories;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.Enumerator.Implementation.Services;
using WB.Core.SharedKernels.Enumerator.Models.Questionnaire;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure.Storage;
using WB.Infrastructure.Shared.Enumerator;
using WB.UI.Interviewer.Implementations.Services;
using WB.UI.Interviewer.Infrastructure.Logging;
using WB.UI.Shared.Enumerator.CustomServices;
using IPrincipal = WB.Core.SharedKernels.Enumerator.Services.Infrastructure.IPrincipal;

namespace WB.UI.Interviewer.Infrastructure
{
    public class InterviewerInfrastructureModule : NinjectModule
    {
        public override void Load()
        {
         //   this.InitilaizeSiaqodb();

            this.Bind<IPlainKeyValueStorage<QuestionnaireModel>>().To<QuestionnaireModelKeyValueStorage>().InSingletonScope();
            this.Bind<IPlainKeyValueStorage<QuestionnaireDocument>>().To<QuestionnaireKeyValueStorage>().InSingletonScope();

            //this.Bind(typeof(IAsyncPlainStorage<>)).To(typeof(SiaqodbPlainStorage<>)).InSingletonScope();

 			this.Bind<IInterviewerQuestionnaireAccessor>().To<InterviewerQuestionnaireAccessor>();
            this.Bind<IInterviewerInterviewAccessor>().To<InterviewerInterviewAccessor>();


            this.Bind<IPlainQuestionnaireRepository>().To<PlainQuestionnaireRepositoryWithCache>();
            this.Bind<IPlainInterviewFileStorage>().To<InterviewerPlainInterviewFileStorage>();

            this.Bind<IEventStore>().To<SiaqodbEventStorage>();

            this.Bind<ISQLitePlatform>().To<SQLitePlatformAndroid>();
            this.Bind<SqliteSettings>().ToConstant(
                new SqliteSettings()
                {
                    PathToDatabaseDirectory = AndroidPathUtils.GetPathToSubfolderInLocalDirectory("data")
                });
            this.Bind(typeof(IAsyncPlainStorage<>)).To(typeof(SqlitePlainStorage<>)).InSingletonScope();

            this.Bind<InterviewerPrincipal>().To<InterviewerPrincipal>().InSingletonScope();
            this.Bind<IPrincipal>().ToMethod<IPrincipal>(context => context.Kernel.Get<InterviewerPrincipal>());
            this.Bind<IInterviewerPrincipal>().ToMethod<IInterviewerPrincipal>(context => context.Kernel.Get<InterviewerPrincipal>());
            
            this.Bind<ILogger>().ToConstant(new FileLogger(AndroidPathUtils.GetPathToFileInLocalSubDirectory("logs", "errors.log")));

            this.Bind<IBackupRestoreService>()
                .To<BackupRestoreService>()
                .WithConstructorArgument("privateStorage", AndroidPathUtils.GetPathToLocalDirectory())
                .WithConstructorArgument("crashFilePath", AndroidPathUtils.GetPathToCrashFile());

            this.Bind<IQuestionnaireAssemblyFileAccessor>().ToConstructor(
                kernel => new InterviewerQuestionnaireAssemblyFileAccessor(kernel.Inject<IFileSystemAccessor>(), 
                kernel.Inject<IAsynchronousFileSystemAccessor>(), kernel.Inject<ILogger>(), 
                AndroidPathUtils.GetPathToSubfolderInLocalDirectory("assemblies")));

            this.Bind<JsonUtilsSettings>().ToSelf().InSingletonScope();
            this.Bind<IProtobufSerializer>().To<ProtobufSerializer>();
            this.Bind<ISerializer>().ToMethod((ctx) => new NewtonJsonSerializer(
                new JsonSerializerSettingsFactory(),
                new Dictionary<string, string>()
                {
                    {
                        "WB.UI.Capi", "WB.Core.BoundedContexts.Interviewer"
                    }
                }));
            this.Bind<IStringCompressor>().To<JsonCompressor>();
        }

        private void InitilaizeSiaqodb()
        {
            this.Bind<IDocumentSerializer>().To<SiaqodbSerializer>();

            SiaqodbConfigurator.SetLicense(
                @"yrwPAibl/TwJ+pR5aBOoYieO0MbZ1HnEKEAwjcoqtdrUJVtXxorrxKZumV+Z48/Ffjj58P5pGVlYZ0G1EoPg0w==");
            SiaqodbConfigurator.SetDocumentSerializer(this.Kernel.Get<IDocumentSerializer>());
            SiaqodbConfigurator.AddDocument("Document", typeof (QuestionnaireDocumentView));
            SiaqodbConfigurator.AddDocument("Model", typeof (QuestionnaireModelView));
            SiaqodbConfigurator.AddText("JsonEvent", typeof (EventView));
            SiaqodbConfigurator.AddText("Title", typeof (QuestionnaireView));
            SiaqodbConfigurator.AddText("LastInterviewerOrSupervisorComment", typeof (InterviewView));
            SiaqodbConfigurator.AddText("QuestionText", typeof (InterviewAnswerOnPrefilledQuestionView));
            SiaqodbConfigurator.AddText("Answer", typeof (InterviewAnswerOnPrefilledQuestionView));
            SiaqodbConfigurator.SpecifyStoredDateTimeKind(DateTimeKind.Utc);
            SiaqodbConfigurator.PropertyUseField("Id", "_id", typeof (IPlainStorageEntity));
            SiaqodbConfigurator.EncryptedDatabase = true;
            SiaqodbConfigurator.SetEncryptionPassword("q=5+yaQqS0K!rWaw8FmLuRDWj8XpwI04Yr4MhtULYmD3zX+W+g");
            SiaqodbConfigurator.AutoGrowthSize = 256*1024*1024;
            
            this.Bind<Siaqodb>().ToConstant(new Siaqodb(AndroidPathUtils.GetPathToSubfolderInLocalDirectory("data")));
        }
    }
}