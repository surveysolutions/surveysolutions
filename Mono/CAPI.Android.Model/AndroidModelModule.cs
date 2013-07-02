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
using AndroidNcqrs.Eventing.Storage.SQLite.DenormalizerStorage;
using CAPI.Android.Core.Model.Authorization;
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
using Ninject.Modules;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;

namespace CAPI.Android.Core.Model
{
    public class AndroidModelModule : NinjectModule
    {
        public override void Load()
        {
            var loginStore = new SqliteReadSideRepositoryAccessor<LoginDTO>();
            Kernel.Bind<IReadSideRepositoryWriter<LoginDTO>>().ToConstant(loginStore);
            Kernel.Bind<IFilterableReadSideRepositoryReader<LoginDTO>>().ToConstant(loginStore);

            var bigSurveyStore = new InMemoryReadSideRepositoryAccessor<CompleteQuestionnaireView>();

            Kernel.Bind<IReadSideRepositoryWriter<CompleteQuestionnaireView>>().ToConstant(bigSurveyStore);
            Kernel.Bind<IReadSideRepositoryReader<CompleteQuestionnaireView>>().ToConstant(bigSurveyStore);

            var surveyStore = new SqliteReadSideRepositoryAccessor<SurveyDto>();
           
            Kernel.Bind<IReadSideRepositoryWriter<SurveyDto>>().ToConstant(surveyStore);

            Kernel.Bind<IFilterableReadSideRepositoryReader<SurveyDto>>().ToConstant(surveyStore);

            var questionnaireStore = new SqliteReadSideRepositoryAccessor<QuestionnaireDTO>();

            Kernel.Bind<IReadSideRepositoryWriter<QuestionnaireDTO>>().ToConstant(questionnaireStore);

            Kernel.Bind<IFilterableReadSideRepositoryReader<QuestionnaireDTO>>().ToConstant(questionnaireStore);

            var publicStore = new SqliteReadSideRepositoryAccessor<PublicChangeSetDTO>();

            Kernel.Bind<IReadSideRepositoryWriter<PublicChangeSetDTO>>().ToConstant(publicStore);

            var draftStore = new SqliteReadSideRepositoryAccessor<DraftChangesetDTO>();

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
        }
    }
}