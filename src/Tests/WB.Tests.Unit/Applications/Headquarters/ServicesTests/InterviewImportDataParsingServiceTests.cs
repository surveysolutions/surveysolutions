using System;
using System.Collections.Generic;
using System.Linq;
using CsQuery.Engine.PseudoClassSelectors;
using Main.Core.Entities.Composite;
using Moq;
using NUnit.Framework;
using WB.Core.BoundedContexts.Headquarters.AssignmentImport;
using WB.Core.BoundedContexts.Headquarters.AssignmentImport.Parser;
using WB.Core.BoundedContexts.Headquarters.Repositories;
using WB.Core.BoundedContexts.Headquarters.Services.Preloading;
using WB.Core.BoundedContexts.Headquarters.Views.User;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.InterviewEntities.Answers;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.DataCollection.ValueObjects;
using WB.Tests.Abc;

namespace WB.Tests.Unit.Applications.Headquarters.ServicesTests
{
    [TestOf(typeof(InterviewImportDataParsingService))]
    internal class InterviewImportDataParsingServiceTests
    {
        private static InterviewImportDataParsingService CreateInterviewImportDataParsingService(
            IPreloadedDataRepository preloadedDataRepository = null,
            IQuestionnaireStorage questionnaireStorage = null,
            IQuestionnaireExportStructureStorage questionnaireExportStructureStorage = null,
            IUserViewFactory userViewFactory = null,
            IQuestionDataParser dataParser = null)
            => new InterviewImportDataParsingService(
                preloadedDataRepository ?? Mock.Of<IPreloadedDataRepository>(),
                questionnaireStorage ?? Mock.Of<IQuestionnaireStorage>(),
                questionnaireExportStructureStorage ?? Mock.Of<IQuestionnaireExportStructureStorage>(),
                userViewFactory ?? Mock.Of<IUserViewFactory>(),
                dataParser ?? Mock.Of<IQuestionDataParser>());

        [Test]
        public void when_GetAssignmentsData_in_assignments_mode_and_preload_file_name_is_not_questionnaire_title_name()
        {
            //arrange
            var importProcessId = "processid";
            var questionnaireIdentity = new QuestionnaireIdentity(Guid.Parse("11111111111111111111111111111111"), 1);
            var preloadedFileName = "test";
            var questionVariable = "qVar";

            var questionnaireDocument = Create.Entity.QuestionnaireDocument(children: new []{ Create.Entity.TextQuestion(variable: questionVariable)});
            var questionnaireStorage = Setup.QuestionnaireRepositoryWithOneQuestionnaire(questionnaireIdentity, questionnaireDocument);

            var questionnaireExportStructure = Mock.Of<IQuestionnaireExportStructureStorage>(x =>
                x.GetQuestionnaireExportStructure(questionnaireIdentity) ==
                Create.Entity.QuestionnaireExportStructure(questionnaireDocument));

            var preloadedDataRepository = Mock.Of<IPreloadedDataRepository>(x =>
                x.GetPreloadedDataOfSample(It.IsAny<string>()) == new PreloadedDataByFile(importProcessId,
                    preloadedFileName, new[] {questionVariable}, new[] {new[] {"text"}}));

            var service = CreateInterviewImportDataParsingService(questionnaireStorage: questionnaireStorage,
                preloadedDataRepository: preloadedDataRepository, questionnaireExportStructureStorage: questionnaireExportStructure);

            //act
            var result = service.GetAssignmentsData(importProcessId, questionnaireIdentity, AssignmentImportType.Assignments);
            //assert
            Assert.That(result, Is.Not.Empty);
        }


        [Test]
        public void when_ParseInterviews_in_assignments_mode_and_preload_file_name_is_not_questionnaire_title_name()
        {
            //arrange
            var questionnaireId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
            var listTopQuestionId = Guid.Parse("22222222222222222222222222222222");
            var listInnerQuestionId = Guid.Parse("33333333333333333333333333333333");
            var topRosterId = Guid.Parse("44444444444444444444444444444444");
            var innerRosterId = Guid.Parse("55555555555555555555555555555555");

            var questionnaireDocument = Create.Entity.QuestionnaireDocument(children: new IComposite[]
            {
                Create.Entity.TextListQuestion(listTopQuestionId, variable: "list_top", maxAnswerCount: 2),
                Create.Entity.ListRoster(topRosterId, variable: "list_roster_top", rosterSizeQuestionId: listTopQuestionId, children: new IComposite[]
                {
                    Create.Entity.TextListQuestion(listInnerQuestionId, variable: "list_inner", maxAnswerCount: 5),
                    Create.Entity.ListRoster(innerRosterId, variable: "list_roster_inner", rosterSizeQuestionId:listInnerQuestionId, children: new []
                    {
                        Create.Entity.TextQuestion(variable: "q1")
                    })
                }),
            });

            var questionnaire = Create.Entity.PlainQuestionnaire(questionnaireDocument);
            var exportStructure = Create.Entity.QuestionnaireExportStructure(questionnaireDocument);
            var service = CreateInterviewImportDataParsingService(dataParser: new QuestionDataParser());

            PreloadedDataByFile preloadedDataByFileMain = new PreloadedDataByFile(questionnaireId.FormatGuid(), null, 
                header: new [] { "interview__id",   "bsid", "list_top__0", "list_top__1" },
                content: new []
                {
                    new []{ "2",   "20000",   "AA",  "AB" }
                });

            PreloadedDataByFile preloadedDataByFileTopRoster = new PreloadedDataByFile(questionnaireId.FormatGuid(), null, 
                header: new []
                {
                    "list_roster_top__id",  "list_top", "list_inner__0", "list_inner__1", "list_inner__2", "list_inner__3", "list_inner__4", "interview__id"
                },
                content: new []
                {
                    new []{ "1",   "AA", "Member 1", "Member 2", "Member 3", "Member 4", "Member 5", "2" },
                    new []{ "2",   "AB", "Member 1", "Member 3", "Member 5", null,        null,      "2" }
                });

            PreloadedDataByFile preloadedDataByFileInsideRoster = new PreloadedDataByFile(questionnaireId.FormatGuid(), null, 
                header: new []
                {
                    "list_roster_inner__id", "list_inner", "q1", "list_roster_top__id", "interview__id"
                },
                content: new []
                {
                    new []{ "1", "Member 1", null, "1", "2" },
                    new []{ "2", "Member 2", null, "1", "2" },
                    new []{ "3", "Member 3", null, "1", "2" },
                    new []{ "4", "Member 4", null, "1", "2" },
                    new []{ "5", "Member 5", null, "1", "2" },
                    new []{ "1", "Member 1", null, "2", "2" },
                    new []{ "2", "Member 3", null, "2", "2" },
                    new []{ "3", "Member 5", null, "2", "2" },
                });

            List<PreloadedInterviewBaseLevel> preloadedInterviewBaseLevels = new List<PreloadedInterviewBaseLevel>()
            {
                new PreloadedInterviewQuestionnaireLevel(preloadedDataByFileMain, exportStructure),
                new PreloadedInterviewLevel(preloadedDataByFileTopRoster, topRosterId, new Guid[] { listTopQuestionId }, exportStructure),
                new PreloadedInterviewLevel(preloadedDataByFileInsideRoster, innerRosterId, new Guid[] { listTopQuestionId, listInnerQuestionId }, exportStructure),
            };

            //act
            var result = service.ParseInterviews(preloadedInterviewBaseLevels, questionnaire);

            //assert
            Assert.That(result, Is.Not.Empty);
            var topTextListAnswer = (TextListAnswer)result[0].Answers[0].Answer;
            Assert.That(topTextListAnswer.Rows.Select(r => new Tuple<int, string>(r.Value, r.Text)), Is.EqualTo(new[]
            {
                new Tuple<int, string>(1, "AA"),
                new Tuple<int, string>(2, "AB"),
            }));

            var innerFirstTextListAnswer = (TextListAnswer)result[0].Answers[1].Answer;
            Assert.That(innerFirstTextListAnswer.Rows.Select(r => new Tuple<int, string>(r.Value, r.Text)), Is.EqualTo(new[]
            {
                new Tuple<int, string>(1, "Member 1"),
                new Tuple<int, string>(2, "Member 2"),
                new Tuple<int, string>(3, "Member 3"),
                new Tuple<int, string>(4, "Member 4"),
                new Tuple<int, string>(5, "Member 5"),
            }));
            var innerSecondTextListAnswer = (TextListAnswer)result[0].Answers[2].Answer;
            Assert.That(innerSecondTextListAnswer.Rows.Select(r => new Tuple<int, string>(r.Value, r.Text)), Is.EqualTo(new []
            {
                new Tuple<int, string>(1, "Member 1"),
                new Tuple<int, string>(2, "Member 3"),
                new Tuple<int, string>(3, "Member 5"),
            }));
        }
    }
}
