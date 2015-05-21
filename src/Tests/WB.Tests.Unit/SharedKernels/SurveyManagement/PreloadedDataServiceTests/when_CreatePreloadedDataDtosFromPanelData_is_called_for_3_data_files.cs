using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Machine.Specifications;
using Main.Core.Documents;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using Main.Core.Entities.SubEntities.Question;
using WB.Core.SharedKernels.DataCollection.DataTransferObjects.Preloading;
using WB.Core.SharedKernels.SurveyManagement.Implementation.Services.Preloading;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.PreloadedDataServiceTests
{
    internal class when_CreatePreloadedDataDtosFromPanelData_is_called_for_3_data_files : PreloadedDataServiceTestContext
    {
        Establish context = () =>
        {
            questionnaireDocument =
                CreateQuestionnaireDocumentWithOneChapter(
                    new NumericQuestion() { StataExportCaption = "nq1", QuestionType = QuestionType.Numeric, PublicKey = Guid.NewGuid() },
                    new TextQuestion() { StataExportCaption = "tq1", QuestionType = QuestionType.Text, PublicKey = Guid.NewGuid() },
                    new Group("Roster Group")
                    {
                        IsRoster = true,
                        VariableName = "Roster Group",
                        RosterSizeSource = RosterSizeSourceType.FixedTitles,
                        PublicKey = rosterGroupId,
                        RosterFixedTitles = new[] { "a", "b" },
                        Children = new List<IComposite>
                        {
                            new NumericQuestion()
                            {
                                StataExportCaption = "nq2",
                                QuestionType = QuestionType.Numeric,
                                PublicKey = Guid.NewGuid()
                            },
                            new Group("nestedRoster")
                            {
                                IsRoster = true,
                                PublicKey = nestedRosterId,
                                RosterSizeSource = RosterSizeSourceType.FixedTitles,
                                RosterFixedTitles = new[] { "1", "2" },
                                VariableName = "nestedRoster",
                                Children = new List<IComposite>
                                {
                                    new NumericQuestion()
                                    {
                                        StataExportCaption = "nq3",
                                        QuestionType = QuestionType.Numeric,
                                        PublicKey = Guid.NewGuid()
                                    }
                                }
                            }
                        }
                    });

            preloadedDataService = CreatePreloadedDataService(questionnaireDocument);
        };

        Because of =
            () =>
                result =
                    preloadedDataService.CreatePreloadedDataDtosFromPanelData(new[]
                    {
                        CreatePreloadedDataByFile(new[] { "Id", "nq1" }, new[]
                        {
                            new[] { "top1", "2" },
                            new[] { "top2", "3" }
                        }, questionnaireDocument.Title),
                        CreatePreloadedDataByFile(new[] { "Id", "nq2", "ParentId1" }, new[]
                        {
                            new[] { "1", "11", "top1" },
                            new[] { "1", "21", "top2" },
                            new[] { "2", "22", "top2" }
                        }, "Roster Group"),
                        CreatePreloadedDataByFile(new[] { "Id", "nq3", "ParentId1", "ParentId2" }, new[]
                        {
                            new[] { "1", "11", "1", "top1" },
                            new[] { "2", "12", "1", "top1" },
                            new[] { "1", "21", "1", "top2" },
                            new[] { "2", "31", "2", "top2" }
                        }, "nestedRoster")
                    });

        It should_return_not_null_result = () =>
            result.ShouldNotBeNull();

        It should_result_has_2_items = () =>
           result.Length.ShouldEqual(2);

        It should_first_result_has_data_for_4_levels_1_top_1_roster_and_2_nested = () =>
           result[0].PreloadedDataDto.Data.Length.ShouldEqual(4);

        It should_first_result_first_data_record_has_empty_roster_vector = () =>
          result[0].PreloadedDataDto.Data[0].RosterVector.SequenceEqual(new decimal[0]).ShouldBeTrue();

        It should_first_result_second_data_record_has_1_roster_vector = () =>
          result[0].PreloadedDataDto.Data[1].RosterVector.SequenceEqual(new decimal[] { 1 }).ShouldBeTrue();

        It should_first_result_thirdt_data_record_has_1_1_roster_vector = () =>
          result[0].PreloadedDataDto.Data[2].RosterVector.SequenceEqual(new decimal[] { 1, 1 }).ShouldBeTrue();

        It should_first_result_fourth_data_record_has_1_2_roster_vector = () =>
         result[0].PreloadedDataDto.Data[3].RosterVector.SequenceEqual(new decimal[] { 1, 2 }).ShouldBeTrue();

        It should_second_result_has_data_for_5_levels_1_top_2_roster_and_2_nested = () =>
          result[1].PreloadedDataDto.Data.Length.ShouldEqual(5);

        It should_second_result_first_data_record_has_empty_roster_vector = () =>
          result[1].PreloadedDataDto.Data[0].RosterVector.SequenceEqual(new decimal[0]).ShouldBeTrue();

        It should_second_result_second_data_record_has_1_roster_vector = () =>
          result[1].PreloadedDataDto.Data[1].RosterVector.SequenceEqual(new decimal[] { 1 }).ShouldBeTrue();

        It should_second_result_thirdt_data_record_has_1_1_roster_vector = () =>
          result[1].PreloadedDataDto.Data[2].RosterVector.SequenceEqual(new decimal[] { 1, 1 }).ShouldBeTrue();

        It should_second_result_fourth_data_record_has_2_roster_vector = () =>
          result[1].PreloadedDataDto.Data[3].RosterVector.SequenceEqual(new decimal[] { 2 }).ShouldBeTrue();

        It should_second_result_fifth_data_record_has_2_2_roster_vector = () =>
          result[1].PreloadedDataDto.Data[4].RosterVector.SequenceEqual(new decimal[] { 2, 2 }).ShouldBeTrue();

        private static PreloadedDataService preloadedDataService;
        private static QuestionnaireDocument questionnaireDocument;
        private static PreloadedDataRecord[] result;
        private static Guid rosterGroupId = Guid.NewGuid();
        private static Guid nestedRosterId = Guid.NewGuid();
    }
}
