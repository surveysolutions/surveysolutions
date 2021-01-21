using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using GeoJSON.Net.Feature;
using GeoJSON.Net.Geometry;
using Main.Core.Entities.SubEntities;
using Microsoft.Extensions.Caching.Memory;
using Supercluster;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Core.BoundedContexts.Headquarters.Views.Questionnaire;
using WB.Core.BoundedContexts.Headquarters.Views.Reposts.InputModels;
using WB.Core.BoundedContexts.Headquarters.Views.Reposts.Views;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;

namespace WB.Core.BoundedContexts.Headquarters.Views.Reposts.Factories
{
    internal class MapReport : IMapReport
    {
        private readonly IInterviewFactory interviewFactory;
        private readonly IPlainStorageAccessor<QuestionnaireBrowseItem> questionnairesAccessor;
        private readonly IPlainStorageAccessor<QuestionnaireCompositeItem> questionnaireItems;
        private readonly IAuthorizedUser authorizedUser;
        private readonly IMemoryCache cache;

        public MapReport(IInterviewFactory interviewFactory,
            IPlainStorageAccessor<QuestionnaireBrowseItem> questionnairesAccessor,
            IAuthorizedUser authorizedUser,
            IMemoryCache cache,
            IPlainStorageAccessor<QuestionnaireCompositeItem> questionnaireItems)
        {
            this.interviewFactory = interviewFactory;
            this.questionnairesAccessor = questionnairesAccessor;
            this.authorizedUser = authorizedUser;
            this.cache = cache;
            this.questionnaireItems = questionnaireItems;
        }

        public List<string> GetGpsQuestionsByQuestionnaire(Guid questionnaireId, long? version)
        {
            var questions = this.questionnaireItems.Query(q =>
            {
                if (version == null)
                {
                    var questionnaire = questionnaireId.FormatGuid();
                    q = q.Where(i => i.QuestionnaireIdentity.StartsWith(questionnaire));
                }
                else
                {
                    var identity = new QuestionnaireIdentity(questionnaireId, version.Value).ToString();
                    q = q.Where(i => i.QuestionnaireIdentity == identity);
                }

                q = q.Where(i => i.QuestionType == QuestionType.GpsCoordinates);

                return q.Select(i => i.StataExportCaption).Distinct().ToList();
            });

            return questions;
        }

        protected static object locker = new object();

        public MapReportView Load(MapReportInputModel input)
        {
            var key = $"MapReport;{input.QuestionnaireId};{input.QuestionnaireVersion ?? 0L};{input.Variable};{this.authorizedUser.Id}";

            var cacheLine = cache.Get(key);

            if (cacheLine == null)
            {
                lock (locker)
                {
                    cacheLine = cache.Get(key);

                    if (cacheLine == null)
                    {
                        var sw = Stopwatch.StartNew();
                        cacheLine = InitializeSuperCluster(input);
                        sw.Stop();

                        // cache for up to 10 minute depending on how long it took to read out map data
                        var cacheTime = TimeSpan.FromMilliseconds(sw.ElapsedMilliseconds * 5 + 1);

                        cache.Set(key, cacheLine, cacheTime);
                    }
                }
            }

            var map = (MapReportCacheLine) cacheLine;

            if (input.Zoom == -1)
            {
                input.Zoom = map.Bounds.ApproximateGoogleMapsZoomLevel(input.ClientMapWidth);
            }

            var result = map.Cluster.GetClusters(new GeoBounds(input.South, input.West, input.North, input.East), input.Zoom);

            var collection = new FeatureCollection();
            collection.Features.AddRange(result.Select(p =>
            {
                var props = p.UserData.Props ?? new Dictionary<string, object>();

                if (p.UserData.NumPoints > 1)
                {
                    props["count"] = p.UserData.NumPoints;
                    props["expand"] = map.Cluster.GetClusterExpansionZoom(p.UserData.Index);
                }

                return new Feature(
                    new Point(new Position(p.Latitude, p.Longitude)),
                    props, id: p.UserData.Index.ToString("X"));
            }));

            return new MapReportView
            {
                FeatureCollection = collection,
                InitialBounds = map.Bounds,
                TotalPoint = map.Total
            };
        }

        private MapReportCacheLine InitializeSuperCluster(MapReportInputModel input)
        {
            var gpsAnswers = this.interviewFactory.GetGpsAnswers(
                input.QuestionnaireId, input.QuestionnaireVersion, input.Variable, null, 
                this.authorizedUser.IsSupervisor ? this.authorizedUser.Id : (Guid?)null);

            var cluster = new SuperCluster();
            var bounds = GeoBounds.Closed;

            cluster.Load(gpsAnswers.Select(g =>
            {
                bounds.Expand(g.Latitude, g.Longitude);
                return new Feature(new Point(new Position(g.Latitude, g.Longitude)),
                    new Dictionary<string, object> { ["interviewId"] = g.InterviewId.ToString() });
            }));

            return new MapReportCacheLine
            {
                Cluster = cluster,
                Bounds = bounds,
                Total = gpsAnswers.Length
            };
        }

        public List<QuestionnaireBrowseItem> GetQuestionnaireIdentitiesWithGpsQuestions()
        {
            var questionnaireIds = this.questionnaireItems.Query(q =>
            {
                return q.Where(item => item.QuestionType == QuestionType.GpsCoordinates)
                    .Select(item => item.QuestionnaireIdentity)
                    .Distinct().ToList();
            });

            return this.questionnairesAccessor.Query(_ => _.Where(x => !x.IsDeleted && questionnaireIds.Contains(x.Id)).ToList());
        }

        private class MapReportCacheLine
        {
            public SuperCluster Cluster { get; set; }
            public GeoBounds Bounds { get; set; }
            public int Total { get; set; }
        }
    }
}
