using System;
using System.Collections.Generic;
using System.Linq;
using Main.Core.View;
using WB.Core.GenericSubdomains.Utils;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;

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
            var result = new DashboardModel(input.UserId);

            var userId = input.UserId.FormatGuid();

            var surveys = surveyDtoDocumentStorage.Filter(s => true).ToList();
            List<QuestionnaireDTO> questionnaires = this.questionnaireDtoDocumentStorage.Filter(q => q.Responsible == userId).ToList();

            foreach (SurveyDto surveyDto in surveys)
            {
                var interviews = new List<QuestionnaireDTO>();
                if (string.IsNullOrEmpty(surveyDto.QuestionnaireId))
                {
                    interviews.AddRange(questionnaires.Where(q => q.Survey == surveyDto.Id));
                }
                else
                {
                    interviews.AddRange(
                        questionnaires.Where(q => q.Survey == surveyDto.QuestionnaireId && q.SurveyVersion == surveyDto.QuestionnaireVersion));
                }
                if (interviews.Any() || surveyDto.AllowCensusMode)
                {
                    result.Surveys.Add(new DashboardSurveyItem(surveyDto.Id, 
                        surveyDto.QuestionnaireId, 
                        surveyDto.QuestionnaireVersion,
                        surveyDto.SurveyTitle, 
                        interviews.Select(i => i.GetDashboardItem(i.Survey, surveyDto.SurveyTitle, i.Comments)),
                        surveyDto.AllowCensusMode));
                }
            }
            return result;
        }

        #endregion
    }
}