using System;
using System.Collections.Generic;
using System.Linq;

using CAPI.Android.Core.Model.ViewModel.Dashboard;

using WB.Core.GenericSubdomains.Utils;
using WB.Core.Infrastructure.ReadSide;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;

namespace WB.UI.Capi.ViewModel.Dashboard
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

        public DashboardModel Load(DashboardInput input)
        {
            var result = new DashboardModel(input.UserId);

            var userId = input.UserId.FormatGuid();

            var surveys = this.surveyDtoDocumentStorage.Filter(s => true).ToList();
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
                        interviews.Select(
                            i =>
                                new DashboardQuestionnaireItem(Guid.Parse(i.Id), Guid.Parse(i.Survey), (InterviewStatus)i.Status,
                                    i.GetProperties(), surveyDto.SurveyTitle, i.Comments, i.CreatedOnClient,
                                    i.JustInitilized.HasValue && i.JustInitilized.Value)),
                        surveyDto.AllowCensusMode));
                }
            }
            return result;
        }
    }
}