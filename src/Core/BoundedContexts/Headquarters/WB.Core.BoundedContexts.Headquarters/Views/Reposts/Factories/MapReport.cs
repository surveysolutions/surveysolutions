using System;
using System.Collections.Generic;
using System.Linq;
using Main.Core.Entities.SubEntities.Question;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Core.BoundedContexts.Headquarters.Views.Questionnaire;
using WB.Core.BoundedContexts.Headquarters.Views.Reposts.InputModels;
using WB.Core.BoundedContexts.Headquarters.Views.Reposts.Views;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.Repositories;

namespace WB.Core.BoundedContexts.Headquarters.Views.Reposts.Factories
{
    internal class MapReport : IMapReport
    {
        private readonly IInterviewFactory interviewFactory;
        private readonly IQuestionnaireStorage questionnaireStorage;
        private readonly IPlainStorageAccessor<QuestionnaireBrowseItem> questionnairesAccessor;
        private const int MAXCOORDINATESCOUNTLIMIT = 50000;

        public MapReport(IInterviewFactory interviewFactory, IQuestionnaireStorage questionnaireStorage,
            IPlainStorageAccessor<QuestionnaireBrowseItem> questionnairesAccessor)
        {
            this.interviewFactory = interviewFactory;
            this.questionnaireStorage = questionnaireStorage;
            this.questionnairesAccessor = questionnairesAccessor;
        }

        public List<string> GetVariablesForQuestionnaire(QuestionnaireIdentity questionnaireIdentity)
        {
            var answeredGpsQuestionIds = this.interviewFactory.GetAnsweredGpsQuestionIdsByQuestionnaire(questionnaireIdentity);

            if (!answeredGpsQuestionIds.Any()) return new List<string>();

            var questionnaire = this.questionnaireStorage.GetQuestionnaireDocument(questionnaireIdentity);
            var gpsQuestions = questionnaire.Find<GpsCoordinateQuestion>().ToArray();

            return answeredGpsQuestionIds.Select(questionId =>
                gpsQuestions.FirstOrDefault(y => y.PublicKey == questionId).StataExportCaption).ToList();
        }

        public MapReportView Load(MapReportInputModel input)
        {
            var questionnaire = this.questionnaireStorage.GetQuestionnaire(input.QuestionnaireIdentity, null);
            var gpsQuestionId = questionnaire.GetQuestionIdByVariable(input.Variable);

            if(!gpsQuestionId.HasValue) throw new ArgumentNullException(nameof(gpsQuestionId));

            var gpsAnswers = this.interviewFactory.GetGpsAnswersByQuestionIdAndQuestionnaire(input.QuestionnaireIdentity,
                gpsQuestionId.Value, MAXCOORDINATESCOUNTLIMIT, input.NorthEastCornerLatitude, input.SouthWestCornerLatitude,
                input.NorthEastCornerLongtitude, input.SouthWestCornerLongtitude);

            return new MapReportView
            {
                Points = gpsAnswers.GroupBy(x => x.InterviewId).Select(x => new MapPointView
                {
                    Id = x.Key.ToString(),
                    Answers = string.Join("|", x.Select(val => $"{val.Latitude};{val.Longitude}"))
                }).ToArray()
            };
        }

        public List<QuestionnaireBrowseItem> GetQuestionnaireIdentitiesWithPoints()
        {
            var questionnaireIdentities = this.interviewFactory.GetQuestionnairesWithAnsweredGpsQuestions();

           return this.questionnairesAccessor.Query(_ => _.Where(x => questionnaireIdentities.Contains(x.Id)).ToList());
        }
    }
}
