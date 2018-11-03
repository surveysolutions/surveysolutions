using System;
using System.Collections.Generic;
using System.Linq;
using GeoJSON.Net.CoordinateReferenceSystem;
using GeoJSON.Net.Feature;
using GeoJSON.Net.Geometry;
using Main.Core.Entities.SubEntities.Question;
using WB.Core.BoundedContexts.Headquarters.Clustering;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Core.BoundedContexts.Headquarters.Views.Questionnaire;
using WB.Core.BoundedContexts.Headquarters.Views.Reposts.InputModels;
using WB.Core.BoundedContexts.Headquarters.Views.Reposts.Views;
using WB.Core.GenericSubdomains.Portable;
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
        private readonly IAuthorizedUser authorizedUser;

        public MapReport(IInterviewFactory interviewFactory, IQuestionnaireStorage questionnaireStorage,
            IPlainStorageAccessor<QuestionnaireBrowseItem> questionnairesAccessor, IAuthorizedUser authorizedUser)
        {
            this.interviewFactory = interviewFactory;
            this.questionnaireStorage = questionnaireStorage;
            this.questionnairesAccessor = questionnairesAccessor;
            this.authorizedUser = authorizedUser;
        }

        public List<string> GetGpsQuestionsByQuestionnaire(QuestionnaireIdentity questionnaireIdentity)
            => this.questionnaireStorage.GetQuestionnaireDocument(questionnaireIdentity)
                .Find<GpsCoordinateQuestion>().Select(question => question.StataExportCaption).ToList();

        public MapReportView Load(MapReportInputModel input)
        {
            var questionnaire = this.questionnaireStorage.GetQuestionnaire(input.QuestionnaireIdentity, null);
            var gpsQuestionId = questionnaire.GetQuestionIdByVariable(input.Variable);

            if(!gpsQuestionId.HasValue) throw new ArgumentNullException(nameof(gpsQuestionId));

            var gpsAnswers = this.interviewFactory.GetGpsAnswers(
                input.QuestionnaireIdentity,
                gpsQuestionId.Value, 0, 180, -180, 180, -180,
                this.authorizedUser.IsSupervisor ? this.authorizedUser.Id : (Guid?) null);

            var superCluster = new SuperCluster();

            (double minLat, double maxLat, double minLong, double maxLong) bounds
                = (double.MaxValue, double.MinValue, double.MaxValue, double.MinValue);

            superCluster.Load(gpsAnswers.Select(g=>
            {
                if (g.Longitude < bounds.minLong) bounds.minLong = g.Longitude;
                if (g.Longitude > bounds.maxLong) bounds.maxLong = g.Longitude;
                if (g.Latitude < bounds.minLat) bounds.minLat = g.Latitude;
                if (g.Latitude > bounds.maxLat) bounds.maxLat = g.Latitude;

                return new SuperCluster.GeoPoint
                {
                    Position = new[] {g.Longitude, g.Latitude},
                    Props = new Dictionary<string, object>
                    {
                        ["interviewId"] = g.InterviewId.ToString(),
                        ["id"] = $"{g.InterviewId.FormatGuid()}_{g.RosterVector}"
                    }
                };
            }));

            var result = superCluster.GetClusters(input.Zoom,
                input.SouthWestCornerLongtitude, input.SouthWestCornerLatitude,
                input.NorthEastCornerLongtitude, input.NorthEastCornerLatitude);

            var collection = new FeatureCollection();

            collection.Features.AddRange(result.Select(p =>
            {
                var props = p.UserData.Props ?? new Dictionary<string, object>();
                props["count"] = p.UserData.NumPoints;

                return new Feature(
                    new Point(new Position(p.Latitude,p.Longitude)),
                    props);
            }));

            return new MapReportView
            {
                InitialBounds = new [] {bounds.minLong, bounds.minLat, bounds.maxLong, bounds.maxLat},
                FeatureCollection = collection,
                TotalPoint = gpsAnswers.Length
            };
        }

        public List<QuestionnaireBrowseItem> GetQuestionnaireIdentitiesWithPoints() =>
            this.questionnairesAccessor.Query(_ => _.Where(x => !x.IsDeleted).ToList());
    }
}
