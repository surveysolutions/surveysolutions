using System;
using System.Linq;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using NUnit.Framework;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.Transactions;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Infrastructure.Native.Storage;
using WB.Tests.Abc;

namespace WB.Tests.Integration
{
    internal partial class InterviewFactoryTests
    {
        private class GpsAnswerDto
        {
            public Guid InterviewId { get; private set; }
            public QuestionnaireIdentity QuestionnaireId { get; private set; }
            public InterviewStateIdentity QuestionId { get; private set; }
            public GeoPosition Answer { get; private set; }
            public Guid TeamLeadId { get; private set; }
            public InterviewStatus InterviewStatus { get; private set; }

            public static GpsAnswerDto Create(Guid? interviewId = null, QuestionnaireIdentity questionnaireId = null,
                InterviewStateIdentity questionId = null, Guid? teamLeadId = null, GeoPosition answer = null,
                InterviewStatus status = InterviewStatus.Completed) =>
                new GpsAnswerDto
                {
                    InterviewId = interviewId ?? Guid.NewGuid(),
                    QuestionnaireId = questionnaireId,
                    QuestionId = questionId,
                    TeamLeadId = teamLeadId ?? Guid.NewGuid(),
                    Answer = answer ?? Abc.Create.Entity.GeoPosition(),
                    InterviewStatus = status
                };
        }

        private InterviewFactory CreateInterviewFactoryAndSaveInterviewStateWithGpsAnswers(GpsAnswerDto[] gpsAnswers)
        {
            var interviewSummaryRepository = GetPostgresInterviewSummaryRepository();
            this.plainTransactionManager.ExecuteInPlainTransaction(() =>
            {
                foreach (var gpsAnswer in gpsAnswers)
                {
                    interviewSummaryRepository.Store(new InterviewSummary
                    {
                        InterviewId = gpsAnswer.InterviewId,
                        ReceivedByInterviewer = false,
                        Status = gpsAnswer.InterviewStatus,
                        QuestionnaireIdentity = gpsAnswer.QuestionnaireId.ToString(),
                        TeamLeadId = gpsAnswer.TeamLeadId

                    }, gpsAnswer.InterviewId.FormatGuid());
                }
            });

            var questionnaire = Create.Entity.QuestionnaireDocumentWithOneChapter(gpsAnswers
                .Select(x => Create.Entity.GpsCoordinateQuestion(x.QuestionId.Id)).OfType<IComposite>().ToArray());

            var factory = CreateInterviewFactory(interviewSummaryRepository: interviewSummaryRepository,
                questionnaireStorage: Create.Fake.QuestionnaireRepositoryWithOneQuestionnaire(questionnaire));

            var entitySerializer = new EntitySerializer<object>();

            this.plainTransactionManager.ExecuteInPlainTransaction(() =>
            {
                foreach (var groupedInterviews in gpsAnswers.GroupBy(x => x.InterviewId))
                {
                    var interviewState = Create.Entity.InterviewState(groupedInterviews.Key);
                    interviewState.Answers = groupedInterviews.ToDictionary(x => x.QuestionId,
                        x => new InterviewStateAnswer
                        {
                            Id = x.QuestionId.Id,
                            RosterVector = x.QuestionId.RosterVector,
                            AsGps = entitySerializer.Serialize(x.Answer)
                        });

                    factory.Save(interviewState);
                }
            });

            return factory;
        }

        [Test]
        public void when_getting_gps_answers_by_questionnaire_id_and_question_id_should_return_specified_answers()
        {
            //arrange
            var questionnaireId = new QuestionnaireIdentity(Guid.NewGuid(), 555);
            var questionId = InterviewStateIdentity.Create(Guid.NewGuid(), Create.RosterVector(1, 2));

            var allGpsAnswers = new[]
            {
                GpsAnswerDto.Create(questionnaireId: questionnaireId, questionId: questionId),
                GpsAnswerDto.Create(questionnaireId: questionnaireId, questionId: questionId),
                GpsAnswerDto.Create(questionnaireId: new QuestionnaireIdentity(Guid.NewGuid(), 777),
                    questionId: InterviewStateIdentity.Create(Guid.NewGuid(), Create.RosterVector(1))),
            };

            var factory = this.CreateInterviewFactoryAndSaveInterviewStateWithGpsAnswers(allGpsAnswers);

            //act
            var gpsAnswers = this.plainTransactionManager.ExecuteInPlainTransaction(
                () => factory.GetGpsAnswers(questionnaireId, questionId.Id, 10, 90, -90, 180, -180, null));

            //assert
            Assert.That(gpsAnswers, Has.Length.EqualTo(2));
            Assert.That(gpsAnswers, Is.EquivalentTo(allGpsAnswers
                    .Where(x => x.QuestionId == questionId && x.QuestionnaireId == questionnaireId)
                    .Select(x => new InterviewGpsAnswer
                    {
                        InterviewId = x.InterviewId,
                        Longitude = x.Answer.Longitude,
                        Latitude = x.Answer.Latitude
                    })));
        }

        [Test]
        public void when_getting_gps_answers_by_questionnaire_id_and_question_id_and_teamlead_id_should_return_specified_answers()
        {
            //arrange
            var questionnaireId = new QuestionnaireIdentity(Guid.NewGuid(), 555);
            var questionId = InterviewStateIdentity.Create(Guid.NewGuid(), Create.RosterVector(1, 2));
            var teamleadid = Guid.Parse("11111111111111111111111111111111");

            var allGpsAnswers = new[]
            {
                GpsAnswerDto.Create(questionnaireId: questionnaireId, questionId: questionId, teamLeadId: teamleadid),
                GpsAnswerDto.Create(questionnaireId: questionnaireId, questionId: questionId),
                GpsAnswerDto.Create(questionnaireId: new QuestionnaireIdentity(Guid.NewGuid(), 777),
                    questionId: InterviewStateIdentity.Create(Guid.NewGuid(), Create.RosterVector(1)), teamLeadId: teamleadid),
            };

            var factory = this.CreateInterviewFactoryAndSaveInterviewStateWithGpsAnswers(allGpsAnswers);

            //act
            var gpsAnswers = this.plainTransactionManager.ExecuteInPlainTransaction(
                () => factory.GetGpsAnswers(questionnaireId, questionId.Id, 10, 90, -90, 180, -180, teamleadid));

            //assert
            Assert.That(gpsAnswers, Has.Length.EqualTo(1));
            Assert.That(gpsAnswers, Is.EquivalentTo(allGpsAnswers
                    .Where(x => x.QuestionId == questionId && x.QuestionnaireId == questionnaireId && x.TeamLeadId == teamleadid)
                    .Select(x => new InterviewGpsAnswer
                    {
                        InterviewId = x.InterviewId,
                        Longitude = x.Answer.Longitude,
                        Latitude = x.Answer.Latitude
                    })));
        }

        [TestCase(InterviewStatus.ApprovedBySupervisor)]
        [TestCase(InterviewStatus.ApprovedByHeadquarters)]
        public void when_getting_gps_answers_by_questionnaire_id_and_question_id_and_teamlead_id_and_interview_status_not_allowed_should_not_return_answers(InterviewStatus status)
        {
            //arrange
            var questionnaireId = new QuestionnaireIdentity(Guid.NewGuid(), 555);
            var questionId = InterviewStateIdentity.Create(Guid.NewGuid(), Create.RosterVector(1, 2));
            var teamleadid = Guid.Parse("11111111111111111111111111111111");
            var interviewid = Guid.Parse("22222222222222222222222222222222");

            var allGpsAnswers = new[]
            {
                GpsAnswerDto.Create(interviewid, questionnaireId, questionId, teamleadid, status: status)
            };

            var factory = this.CreateInterviewFactoryAndSaveInterviewStateWithGpsAnswers(allGpsAnswers);

            //act
            var gpsAnswers = this.plainTransactionManager.ExecuteInPlainTransaction(
                () => factory.GetGpsAnswers(questionnaireId, questionId.Id, 10, 90, -90, 180, -180, teamleadid));

            //assert
            Assert.IsEmpty(gpsAnswers);
        }

        [Test]
        public void when_getting_gps_answers_by_questionnaire_id_and_question_id_and_limit_by_west_latitude()
        {
            //arrange
            var questionnaireId = new QuestionnaireIdentity(Guid.NewGuid(), 555);
            var questionId = InterviewStateIdentity.Create(Guid.NewGuid(), Create.RosterVector(1, 2));

            var allGpsAnswers = new[]
            {
                GpsAnswerDto.Create(questionnaireId: questionnaireId, questionId: questionId, answer: Create.Entity.GeoPosition(1)),
                GpsAnswerDto.Create(questionnaireId: new QuestionnaireIdentity(Guid.NewGuid(), 777),
                    questionId: InterviewStateIdentity.Create(Guid.NewGuid(), Create.RosterVector(1)), answer: Create.Entity.GeoPosition(2)),
                GpsAnswerDto.Create(questionnaireId: questionnaireId, questionId: questionId, answer: Create.Entity.GeoPosition(3))
            };

            var factory = this.CreateInterviewFactoryAndSaveInterviewStateWithGpsAnswers(allGpsAnswers);

            //act
            var gpsAnswers = this.plainTransactionManager.ExecuteInQueryTransaction(
                () => factory.GetGpsAnswers(questionnaireId, questionId.Id, 10, 90, 2, 180, -180, null));

            //assert
            Assert.That(gpsAnswers, Has.Length.EqualTo(1));
            Assert.AreEqual(gpsAnswers[0],
                new InterviewGpsAnswer
                {
                    InterviewId = allGpsAnswers[2].InterviewId,
                    Longitude = allGpsAnswers[2].Answer.Longitude,
                    Latitude = allGpsAnswers[2].Answer.Latitude
                });
        }

        [Test]
        public void when_getting_gps_answers_by_questionnaire_id_and_question_id_and_limit_by_answers_count_should_return_specified_answers()
        {
            //arrange
            var questionnaireId = new QuestionnaireIdentity(Guid.NewGuid(), 555);
            var questionId = InterviewStateIdentity.Create(Guid.NewGuid(), Create.RosterVector(1, 2));

            var allGpsAnswers = new[]
            {
                GpsAnswerDto.Create(questionnaireId: questionnaireId, questionId: questionId),
                GpsAnswerDto.Create(questionnaireId: questionnaireId, questionId: questionId),
                GpsAnswerDto.Create(questionnaireId: new QuestionnaireIdentity(Guid.NewGuid(), 777),
                    questionId: InterviewStateIdentity.Create(Guid.NewGuid(), Create.RosterVector(1)))
            };

            var factory = this.CreateInterviewFactoryAndSaveInterviewStateWithGpsAnswers(allGpsAnswers);

            //act
            var gpsAnswers = this.plainTransactionManager.ExecuteInQueryTransaction(
                () => factory.GetGpsAnswers(questionnaireId, questionId.Id, 1, 90, -90, 180, -180, null));

            //assert
            Assert.That(gpsAnswers, Has.Length.EqualTo(1));
        }
    }
}