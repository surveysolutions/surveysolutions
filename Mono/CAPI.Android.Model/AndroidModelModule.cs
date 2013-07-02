using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using AndroidNcqrs.Eventing.Storage.SQLite;
using AndroidNcqrs.Eventing.Storage.SQLite.DenormalizerStorage;
using CAPI.Android.Core.Model.Authorization;
using CAPI.Android.Core.Model.Backup;
using CAPI.Android.Core.Model.ChangeLog;
using CAPI.Android.Core.Model.FileStorage;
using CAPI.Android.Core.Model.ViewModel.Dashboard;
using CAPI.Android.Core.Model.ViewModel.Login;
using CAPI.Android.Core.Model.ViewModel.QuestionnaireDetails;
using CAPI.Android.Core.Model.ViewModel.Statistics;
using CAPI.Android.Core.Model.ViewModel.Synchronization;
using Main.Core.Services;
using Main.Core.View;
using Main.DenormalizerStorage;
using Ncqrs.Eventing.Storage;
using Ninject.Modules;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;

namespace CAPI.Android.Core.Model
{
    public class AndroidModelModule : NinjectModule
    {
        private const string ProjectionStoreName = "Projections";
        private const string EventStoreDatabaseName = "EventStore";


        public override void Load()
        {
            this.Bind<IEventStore>().ToConstant(new MvvmCrossSqliteEventStore(EventStoreDatabaseName));

            var loginStore = new SqliteReadSideRepositoryAccessor<LoginDTO>(ProjectionStoreName);
            Kernel.Bind<IReadSideRepositoryWriter<LoginDTO>>().ToConstant(loginStore);
            Kernel.Bind<IFilterableReadSideRepositoryReader<LoginDTO>>().ToConstant(loginStore);

            var bigSurveyStore = new InMemoryReadSideRepositoryAccessor<CompleteQuestionnaireView>();

            Kernel.Bind<IReadSideRepositoryWriter<CompleteQuestionnaireView>>().ToConstant(bigSurveyStore);
            Kernel.Bind<IReadSideRepositoryReader<CompleteQuestionnaireView>>().ToConstant(bigSurveyStore);

            var surveyStore = new SqliteReadSideRepositoryAccessor<SurveyDto>(ProjectionStoreName);
           
            Kernel.Bind<IReadSideRepositoryWriter<SurveyDto>>().ToConstant(surveyStore);

            Kernel.Bind<IFilterableReadSideRepositoryReader<SurveyDto>>().ToConstant(surveyStore);

            var questionnaireStore = new SqliteReadSideRepositoryAccessor<QuestionnaireDTO>(ProjectionStoreName);

            Kernel.Bind<IReadSideRepositoryWriter<QuestionnaireDTO>>().ToConstant(questionnaireStore);

            Kernel.Bind<IFilterableReadSideRepositoryReader<QuestionnaireDTO>>().ToConstant(questionnaireStore);

            var publicStore = new SqliteReadSideRepositoryAccessor<PublicChangeSetDTO>(ProjectionStoreName);

            Kernel.Bind<IReadSideRepositoryWriter<PublicChangeSetDTO>>().ToConstant(publicStore);

            var draftStore = new SqliteReadSideRepositoryAccessor<DraftChangesetDTO>(ProjectionStoreName);

            Kernel.Bind<IFilterableReadSideRepositoryWriter<DraftChangesetDTO>>().ToConstant(draftStore);

            var fileSystem = new FileStorageService();

            Kernel.Bind<IFileStorageService>().ToConstant(fileSystem);


            Kernel.Bind<IChangeLogManipulator>().To<ChangeLogManipulator>().InSingletonScope();
            Kernel.Bind<IAuthentication>().To<AndroidAuthentication>().InSingletonScope();
            Kernel.Bind<IChangeLogStore>().To<FileChangeLogStore>().InSingletonScope();

            Kernel.Bind<IViewFactory<DashboardInput, DashboardModel>>().To<DashboardFactory>();
            Kernel.Bind<IViewFactory<QuestionnaireScreenInput, CompleteQuestionnaireView>>()
                  .To<QuestionnaireScreenViewFactory>();
            Kernel.Bind<IViewFactory<StatisticsInput, StatisticsViewModel>>().To<StatisticsViewFactory>();


            this.Bind<IBackup>().ToConstant(new SqliteBackup(EventStoreDatabaseName, ProjectionStoreName));
        }
    }
}