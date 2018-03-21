using System;
using System.Linq;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using Main.Core.Entities.SubEntities.Question;
using NUnit.Framework;
using WB.Core.GenericSubdomains.Portable.Implementation.ServiceVariables;
using WB.Tests.Abc;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.PreloadedDataServiceTests
{
    internal class
        when_CreatePreloadedDataDtosFromPanelData_is_called_for_3_data_files : PreloadedDataServiceTestContext
    {
        [Test]
        public void Should_not_return_null_result()
        {
            var rosterGroupId = Guid.NewGuid();
            var nestedRosterId = Guid.NewGuid();

            var questionnaireDocument =
                    CreateQuestionnaireDocumentWithOneChapter(
                        new NumericQuestion()
                        {
                            StataExportCaption = "nq1",
                            QuestionType = QuestionType.Numeric,
                            PublicKey = Guid.NewGuid()
                        },
                        new TextQuestion()
                        {
                            StataExportCaption = "tq1",
                            QuestionType = QuestionType.Text,
                            PublicKey = Guid.NewGuid()
                        },
                        Create.Entity.FixedRoster(rosterId: rosterGroupId,
                            obsoleteFixedTitles: new[] {"a", "b"}, title: "Roster Group", variable: "roster",
                            children: new IComposite[]
                            {
                                new NumericQuestion()
                                {
                                    StataExportCaption = "nq2",
                                    QuestionType = QuestionType.Numeric,
                                    PublicKey = Guid.NewGuid()
                                },
                                Create.Entity.FixedRoster(rosterId: nestedRosterId, title: "nestedRoster",
                                    variable: "nestedRoster",
                                    obsoleteFixedTitles: new[] {"1", "2"},
                                    children: new IComposite[]
                                    {
                                        new NumericQuestion()
                                        {
                                            StataExportCaption = "nq3",
                                            QuestionType = QuestionType.Numeric,
                                            PublicKey = Guid.NewGuid()
                                        }
                                    })
                            }));

            var  importDataParsingService = CreatePreloadedDataService(questionnaireDocument);
            
            //act
            var result = importDataParsingService.CreatePreloadedDataDtosFromPanelData(Create.Entity.PreloadedDataByFile(
                            CreatePreloadedDataByFile(new[] {ServiceColumns.InterviewId, "nq1"}, new[]
                            {
                                new[] {"top1", "2"},
                                new[] {"top2", "3"}
                            }, questionnaireDocument.Title),
                            CreatePreloadedDataByFile(new[] {"roster__id", "nq2", ServiceColumns.InterviewId}, new[]
                            {
                                new[] {"1", "11", "top1"},
                                new[] {"1", "21", "top2"},
                                new[] {"2", "22", "top2"}
                            }, "roster"),
                            CreatePreloadedDataByFile(
                                new[] {"nestedroster__id", "nq3", "roster__id", ServiceColumns.InterviewId}, new[]
                                {
                                    new[] {"1", "11", "1", "top1"},
                                    new[] {"2", "12", "1", "top1"},
                                    new[] {"1", "21", "1", "top2"},
                                    new[] {"2", "31", "2", "top2"}
                                }, "nestedroster")
                        ));

            Assert.IsNotNull(result);
            Assert.AreEqual(result.Length, 2);
            Assert.AreEqual(result[0].PreloadedDataDto.Data.Length, 4);
            Assert.IsTrue(result[0].PreloadedDataDto.Data[0].RosterVector.SequenceEqual(new decimal[0]));
            Assert.IsTrue(result[0].PreloadedDataDto.Data[1].RosterVector.SequenceEqual(new decimal[] {1}));
            Assert.IsTrue(result[0].PreloadedDataDto.Data[2].RosterVector.SequenceEqual(new decimal[] {1, 1}));
            Assert.IsTrue(result[0].PreloadedDataDto.Data[3].RosterVector.SequenceEqual(new decimal[] {1, 2}));

            Assert.AreEqual(result[1].PreloadedDataDto.Data.Length, 5);

            Assert.IsTrue(result[1].PreloadedDataDto.Data[0].RosterVector.SequenceEqual(new decimal[0]));
            Assert.IsTrue(result[1].PreloadedDataDto.Data[1].RosterVector.SequenceEqual(new decimal[] {1}));
            Assert.IsTrue(result[1].PreloadedDataDto.Data[2].RosterVector.SequenceEqual(new decimal[] {1, 1}));
            Assert.IsTrue(result[1].PreloadedDataDto.Data[3].RosterVector.SequenceEqual(new decimal[] {2}));
            Assert.IsTrue(
                result[1].PreloadedDataDto.Data[4].RosterVector.SequenceEqual(new decimal[] {2, 2}));
    }
}
}
