using System.Collections.Generic;
using System.Linq;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Core.BoundedContexts.Headquarters.Views.Questionnaire;
using WB.Core.BoundedContexts.Headquarters.Views.Reposts.InputModels;
using WB.Core.BoundedContexts.Headquarters.Views.Reposts.Views;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;

namespace WB.Core.BoundedContexts.Headquarters.Views.Reposts.Factories
{
    internal class MapReport : IMapReport
    {
        private readonly IQueryableReadSideRepositoryReader<MapReportPoint> mapPointsReader;
        private readonly IPlainStorageAccessor<QuestionnaireBrowseItem> questionnairesAccessor;
        private const int MAXCOORDINATESCOUNTLIMIT = 50000;

        public MapReport(
            IQueryableReadSideRepositoryReader<MapReportPoint> mapPointsReader, 
            IPlainStorageAccessor<QuestionnaireBrowseItem> questionnairesAccessor)
        {
            this.mapPointsReader = mapPointsReader;
            this.questionnairesAccessor = questionnairesAccessor;
        }

        public List<string> GetVariablesForQuestionnaire(QuestionnaireIdentity questionnaireIdentity)
        {
            var variables = this.mapPointsReader.Query(_ => _
               .Where(x => x.QuestionnaireId == questionnaireIdentity.QuestionnaireId && x.QuestionnaireVersion == questionnaireIdentity.Version)
               .GroupBy(x => x.Variable)
               .Select(group => group.Key)
               .ToList());

            return variables;
        }

        public MapReportView Load(MapReportInputModel input)
        {
            var points = this.mapPointsReader.Query(_ =>
                 this.AddMapFilterCondition(_, input)
                    .Where(x => x.QuestionnaireId == input.QuestionnaireIdentity.QuestionnaireId 
                             && x.QuestionnaireVersion == input.QuestionnaireIdentity.Version 
                             && x.Variable == input.Variable)
                    .Select(x => new { x.InterviewId, x.Latitude, x.Longitude })
                    .Take(MAXCOORDINATESCOUNTLIMIT)
                    .ToList());

            var mapPointViews = points.GroupBy(x => x.InterviewId).Select(x => new MapPointView
            {
                Id = x.Key.ToString(),
                Answers = string.Join("|", x.Select(val => $"{val.Latitude};{val.Longitude}"))
            }).ToArray();
            return new MapReportView
            {
                Points = mapPointViews
            };
        }

        public List<QuestionnaireBrowseItem> GetQuestionnaireIdentitiesWithPoints()
        {
            var questionnaireIdentities = this.mapPointsReader.Query(_ => _
                .GroupBy(x=> new { x.QuestionnaireId, x.QuestionnaireVersion })
                .Select(group => new QuestionnaireIdentity(group.Key.QuestionnaireId, group.Key.QuestionnaireVersion).ToString())
                .ToList());

            var questionnaires = questionnairesAccessor.Query(_ => _.Where(x => questionnaireIdentities.Contains(x.Id)).ToList());

            return questionnaires;
        }

        private IQueryable<MapReportPoint> AddMapFilterCondition(IQueryable<MapReportPoint> _, MapReportInputModel input)
        {
            IQueryable<MapReportPoint> mapPoints;
            if (input.NorthEastCornerLongtitude >= input.SouthWestCornerLongtitude)
            {
                mapPoints = _.Where(x => x.Longitude > input.SouthWestCornerLongtitude &&
                                         x.Longitude < input.NorthEastCornerLongtitude);
            }
            else
            {
                mapPoints = _.Where(x => x.Longitude > input.SouthWestCornerLongtitude ||
                                         x.Longitude < input.NorthEastCornerLongtitude);
            }
            mapPoints = mapPoints.Where(x => x.Latitude > input.SouthWestCornerLatitude &&
                                             x.Latitude < input.NorthEastCornerLatitude);

            return mapPoints;
        }
    }
}
