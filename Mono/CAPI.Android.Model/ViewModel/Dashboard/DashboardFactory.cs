using System;
using System.Linq;
using Main.Core.View;
using Main.DenormalizerStorage;

namespace CAPI.Android.Core.Model.ViewModel.Dashboard
{
    public class DashboardFactory : IViewFactory<DashboardInput, DashboardModel>
    {

       private readonly IDenormalizerStorage<QuestionnaireDTO> _questionnaireDTOdocumentStorage;
        private readonly IDenormalizerStorage<SurveyDto> _surveyDTOdocumentStorage;

        public DashboardFactory(IDenormalizerStorage<QuestionnaireDTO> questionnaireDTOdocumentStorage,
            IDenormalizerStorage<SurveyDto> surveyDTOdocumentStorage
            )
        {
            _questionnaireDTOdocumentStorage = questionnaireDTOdocumentStorage;
            _surveyDTOdocumentStorage = surveyDTOdocumentStorage;
        }


        #region Implementation of IViewFactory<DashboardInput,DashboardModel>

        public DashboardModel Load(DashboardInput input)
        {
            var allQuestionnairies =
                _questionnaireDTOdocumentStorage.Query();
            var questionnairies =
                _questionnaireDTOdocumentStorage.Query().Where(q => q.Responsible == input.UserId.ToString()).ToList();
            var result = new DashboardModel(input.UserId);
            var surveysIds = questionnairies.Select(q => q.Survey).Distinct().ToList();
            var surveys = _surveyDTOdocumentStorage.Query().Where(s => surveysIds.Contains(s.Id));

            foreach (SurveyDto surveyDto in surveys)
            {
                result.Surveys.Add(new DashboardSurveyItem(Guid.Parse(surveyDto.Id), surveyDto.SurveyTitle,
                                                           questionnairies.Where(q => q.Survey == surveyDto.Id)
                                                                          .Select(
                                                                              q => q.GetDashboardItem())
                                       ));
            }
            return result;
            // return _documentStorage.GetByGuid(input.UserId);
            // return _documentStorage.Query().First();

        }

        #endregion
    }
}