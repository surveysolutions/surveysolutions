using System.Linq;
using Moq;
using NUnit.Framework;
using Supercluster;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Core.BoundedContexts.Headquarters.Views.Reposts.Factories;
using WB.Core.BoundedContexts.Headquarters.Views.Reposts.InputModels;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Tests.Abc;

namespace WB.Tests.Unit.BoundedContexts.Headquarters.Clustering
{
    public class MapReportTests
    {
        private IQuestionnaireStorage questionnaireStorage;
        private IAuthorizedUser authorizedUser;
        private QuestionnaireIdentity questionnaireId;
        private MapReport subject;
        private InterviewGpsAnswer[] points;

        [SetUp]
        public void Context()
        {
            questionnaireId = Create.Entity.QuestionnaireIdentity(Id.gA, 1);

            PlainQuestionnaire questionnaire =
                Create.Entity.PlainQuestionnaire(
                    Create.Entity.QuestionnaireDocument(questionnaireId.QuestionnaireId,
                        Create.Entity.GpsCoordinateQuestion(Id.gF, "gps")));

            questionnaireStorage = Mock.Of<IQuestionnaireStorage>(qs => qs.GetQuestionnaire(questionnaireId, null) == questionnaire);
            authorizedUser = Mock.Of<IAuthorizedUser>(a => a.Id == Id.g2);

            Because();
        }

        public void Because()
        {
            this.points = new[]
            {
                Create.Entity.InterviewGpsAnswer(Id.g1, 1,2),
                Create.Entity.InterviewGpsAnswer(Id.g2, 3,4),
                Create.Entity.InterviewGpsAnswer(Id.g3, 7,6),
                Create.Entity.InterviewGpsAnswer(Id.g4, 7,7)
            };

            var interviewFactory = Mock.Of<IInterviewFactory>(
                iif => iif.GetGpsAnswers(questionnaireId, Id.gF, null, It.IsAny<GeoBounds>(), null) == points);

            this.subject = Create.Service.MapReport(
                authorizedUser: authorizedUser,
                interviewFactory: interviewFactory,
                questionnaireStorage: questionnaireStorage);
        }

        [Test]
        public void ShouldClusterGpsAnswers()
        {
            var answers = subject.Load(new MapReportInputModel
            {
                West = -180,
                East = 180,
                North = 90,
                South = -90,
                QuestionnaireId = questionnaireId.ToString(),
                Variable = "gps",
                Zoom = 5
            });

            Assert.That(answers.TotalPoint, Is.EqualTo(4), "Should report total gps points regardless of clustering");

            var features = answers.FeatureCollection.Features;
            
            // first two points are not in cluster
            // last two points are placed near each other and should be clustered
            
            Assert.That(features[0].Properties["interviewId"], Is.EqualTo(points[0].InterviewId.ToString()));
            Assert.That(features[1].Properties["interviewId"], Is.EqualTo(points[1].InterviewId.ToString()));

            Assert.That(features[2].Properties["count"], Is.EqualTo(2));
        }

        [Test]
        public void ShouldReturnExpandPropertyWithProperZoomValue()
        {
            var answers = subject.Load(new MapReportInputModel
            {
                West = -180,
                East = 180,
                North = 90,
                South = -90,
                QuestionnaireId = questionnaireId.ToString(),
                Variable = "gps",
                Zoom = 5
            });

            var expand = answers.FeatureCollection.Features[2].Properties["expand"];
            Assert.That(expand, Is.EqualTo(6));

            var zoomed = subject.Load(new MapReportInputModel
            {
                West = 5,
                East = 8,
                North = 8,
                South = 5,
                QuestionnaireId = questionnaireId.ToString(),
                Variable = "gps",
                Zoom = (int) expand
            });

            var zoomedPoints = zoomed.FeatureCollection.Features;

            Assert.That(zoomedPoints, Has.Count.EqualTo(2));

            Assert.True(zoomedPoints.All(z => z.Properties.ContainsKey("interviewId")), "All returned points are not clusters");
        }
    }
}
