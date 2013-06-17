using System;
using System.Linq;
using Main.Core.View;
using Main.DenormalizerStorage;

using WB.Core.Infrastructure;
using WB.Core.Infrastructure.ReadSide;

namespace CAPI.Android.Core.Model.ViewModel.Dashboard
{
    public class DashboardFactory : IViewFactory<DashboardInput, DashboardModel>
    {
        private readonly IFilterableReadSideRepositoryReader<QuestionnaireDTO> questionnaireDtoDocumentStorage;
        private readonly IFilterableReadSideRepositoryReader<SurveyDto> surveyDtoDocumentStorage;

        public DashboardFactory(IFilterableReadSideRepositoryReader<QuestionnaireDTO> questionnaireDtoDocumentStorage,
            IFilterableReadSideRepositoryReader<SurveyDto> surveyDtoDocumentStorage)
        {
            this.questionnaireDtoDocumentStorage = questionnaireDtoDocumentStorage;
            this.surveyDtoDocumentStorage = surveyDtoDocumentStorage;
        }


        #region Implementation of IViewFactory<DashboardInput,DashboardModel>

        public DashboardModel Load(DashboardInput input)
        {
            var userId = input.UserId.ToString();
            var questionnairies =
                this.questionnaireDtoDocumentStorage.Filter(q => q.Responsible == userId).ToList();
            var result = new DashboardModel(input.UserId);
            var surveysIds = questionnairies.Select(q => q.Survey).Distinct().ToList();
            var surveys = this.surveyDtoDocumentStorage.Filter(s => surveysIds.Contains(s.Id));

            foreach (SurveyDto surveyDto in surveys)
            {
                result.Surveys.Add(new DashboardSurveyItem(Guid.Parse(surveyDto.Id), surveyDto.SurveyTitle,
                                                           questionnairies.Where(q => q.Survey == surveyDto.Id)
                                                                          .Select(
                                                                              q => q.GetDashboardItem(q.Survey, surveyDto.SurveyTitle))
                                       ));
            }
            return result;
            // return _documentStorage.GetById(input.UserId);
            // return _documentStorage.Query().First();

        }

        #endregion
    }
}