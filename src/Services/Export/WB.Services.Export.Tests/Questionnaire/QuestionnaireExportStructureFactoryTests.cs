using System;
using System.Linq;
using NUnit.Framework;
using WB.Services.Export.Interview;
using WB.Services.Export.Questionnaire;
using WB.Services.Export.Tests.Questionnaire.ExportStructureFactoryTests;

namespace WB.Services.Export.Tests.Questionnaire
{
    [TestOf(typeof(QuestionnaireExportStructureFactory))]
    internal class QuestionnaireExportStructureFactoryTests : ExportViewFactoryTestsContext
    {
        [Test]
        public void should_fill_multioption_question_header_title()
        {
            // arrange
            var multiOptionLinkedQuestionId = Guid.Parse("d7127d06-5668-4fa3-b255-8a2a0aaaa020");
            var linkedSourceQuestionId = Guid.NewGuid();

            var questionnaire = Create.QuestionnaireDocumentWithOneChapter(
                Create.Roster(rosterId: Guid.NewGuid(),
                    variable: "row",
                    fixedTitles: new FixedRosterTitle[] { new FixedRosterTitle(1, "1"), new FixedRosterTitle(2, "2") },
                    children: new[]
                    {
                        Create.TextQuestion(id: linkedSourceQuestionId, variable: "varTxt")
                    }),
                Create.MultyOptionsQuestion(id: multiOptionLinkedQuestionId,
                    variable: "mult",
                    linkedToQuestionId: linkedSourceQuestionId));

            var QuestionnaireExportStructureFactory = CreateExportViewFactory();


            // act
            var questionnaireExportStructure = QuestionnaireExportStructureFactory.CreateQuestionnaireExportStructure(questionnaire);

            // assert
            HeaderStructureForLevel headerStructureForLevel = questionnaireExportStructure.HeaderToLevelMap[new ValueVector<Guid>()];
            ExportedQuestionHeaderItem exportedQuestionHeaderItem = headerStructureForLevel.HeaderItems[multiOptionLinkedQuestionId] as ExportedQuestionHeaderItem;

            Assert.That(exportedQuestionHeaderItem.ColumnHeaders.Count, Is.EqualTo(2));
            Assert.That(exportedQuestionHeaderItem.ColumnHeaders.Select(x => x.Name).ToArray(), Is.EquivalentTo(new[] { "mult__0", "mult__1" }));

            Assert.That(exportedQuestionHeaderItem.ColumnHeaders[0].ExportType, Is.EqualTo(ExportValueType.NumericInt));
            Assert.That(exportedQuestionHeaderItem.ColumnHeaders[1].ExportType, Is.EqualTo(ExportValueType.NumericInt));

            Assert.That(exportedQuestionHeaderItem.QuestionSubType, Is.EqualTo(QuestionSubtype.MultiOptionLinkedFirstLevel));
            Assert.That(exportedQuestionHeaderItem.QuestionType, Is.EqualTo(QuestionType.MultyOption));
        }

        [Test]
        public void when_creating_interview_export_view_by_interview_with_linked_multi_question_on_first_level_referenced_on_third()
        {
            var linkedQuestionSourceId = Guid.Parse("12222222222222222222222222222222");
            var rosterId = Guid.Parse("13333333333333333333333333333333");
            var nestedRosterId = Guid.Parse("23333333333333333333333333333333");

            var linkedQuestionId = Guid.Parse("10000000000000000000000000000000");

            var questionnaire = Create.QuestionnaireDocumentWithOneChapter(
                Create.Roster(rosterId: rosterId, fixedTitles: new FixedRosterTitle[] { new FixedRosterTitle(1, "t1"), new FixedRosterTitle(2, "t2")},
                    children: new IQuestionnaireEntity[]
                    {
                        Create.Roster(rosterId: nestedRosterId, fixedTitles: new FixedRosterTitle[] { new FixedRosterTitle(1, "n1"), new FixedRosterTitle(2, "n2")},
                            children: new IQuestionnaireEntity[]
                            {
                                Create.NumericIntegerQuestion(id: linkedQuestionSourceId, variable: "q1")
                            })
                    }),
                Create.MultyOptionsQuestion(id: linkedQuestionId, linkedToQuestionId:linkedQuestionSourceId, variable: "multi")
            );
            var questionnaireExportStructureFactory = CreateExportViewFactory();


            // act
            var questionnaireExportStructure = questionnaireExportStructureFactory.CreateQuestionnaireExportStructure(questionnaire);

            // assert
            HeaderStructureForLevel headerStructureForLevel = questionnaireExportStructure.HeaderToLevelMap[Create.ValueVector()];
            ExportedQuestionHeaderItem exportedQuestionHeaderItem = headerStructureForLevel.HeaderItems[linkedQuestionId] as ExportedQuestionHeaderItem;

            Assert.That(exportedQuestionHeaderItem.ColumnHeaders.Count, Is.EqualTo(4));
            Assert.That(exportedQuestionHeaderItem.ColumnHeaders.Select(x => x.Name).ToArray(), Is.EquivalentTo(new[] { "multi__0", "multi__1", "multi__2", "multi__3" }));

            Assert.That(exportedQuestionHeaderItem.ColumnHeaders[0].ExportType, Is.EqualTo(ExportValueType.String));
            Assert.That(exportedQuestionHeaderItem.ColumnHeaders[1].ExportType, Is.EqualTo(ExportValueType.String));
            Assert.That(exportedQuestionHeaderItem.ColumnHeaders[2].ExportType, Is.EqualTo(ExportValueType.String));
            Assert.That(exportedQuestionHeaderItem.ColumnHeaders[3].ExportType, Is.EqualTo(ExportValueType.String));

            Assert.That(exportedQuestionHeaderItem.QuestionSubType, Is.EqualTo(QuestionSubtype.MultiOptionLinkedNestedLevel));
            Assert.That(exportedQuestionHeaderItem.QuestionType, Is.EqualTo(QuestionType.MultyOption));
        }     
        
        [Test]
        public void when_creating_interview_export_view_by_interview_with_linked_multi_question_on_second_level_referenced_on_fourth()
        {
            var linkedQuestionSourceId = Guid.Parse("99999999999999999999999999999999");
            var roster1Id = Guid.Parse("11111111111111111111111111111111");
            var nested2Id = Guid.Parse("22222222222222222222222222222222");
            var nested3Id = Guid.Parse("33333333333333333333333333333333");

            var linkedQuestionId = Guid.Parse("10000000000000000000000000000000");

            var questionnaire = Create.QuestionnaireDocumentWithOneChapter(
                Create.Roster(rosterId: roster1Id, fixedTitles: new FixedRosterTitle[] { new FixedRosterTitle(1, "f1"), new FixedRosterTitle(2, "f2")},
                    children: new IQuestionnaireEntity[]
                    {
                        Create.Roster(rosterId: nested2Id, fixedTitles: new FixedRosterTitle[] { new FixedRosterTitle(1, "s1"), new FixedRosterTitle(2, "s2")},
                            children: new IQuestionnaireEntity[]
                            {
                                Create.Roster(rosterId: nested3Id, fixedTitles: new FixedRosterTitle[] { new FixedRosterTitle(1, "t1"), new FixedRosterTitle(2, "t2")},
                                    children: new IQuestionnaireEntity[]
                                    {
                                        Create.NumericIntegerQuestion(id: linkedQuestionSourceId, variable: "q1")
                                    })
                            }),
                        Create.MultyOptionsQuestion(id: linkedQuestionId, linkedToQuestionId:linkedQuestionSourceId, variable: "multi")
                    })
            );
            var questionnaireExportStructureFactory = CreateExportViewFactory();


            // act
            var questionnaireExportStructure = questionnaireExportStructureFactory.CreateQuestionnaireExportStructure(questionnaire);

            // assert
            HeaderStructureForLevel headerStructureForLevel = questionnaireExportStructure.HeaderToLevelMap[Create.ValueVector(roster1Id)];
            ExportedQuestionHeaderItem exportedQuestionHeaderItem = headerStructureForLevel.HeaderItems[linkedQuestionId] as ExportedQuestionHeaderItem;

            Assert.That(exportedQuestionHeaderItem.ColumnHeaders.Count, Is.EqualTo(4));
            Assert.That(exportedQuestionHeaderItem.ColumnHeaders.Select(x => x.Name).ToArray(), Is.EquivalentTo(new[] { "multi__0", "multi__1", "multi__2", "multi__3" }));

            Assert.That(exportedQuestionHeaderItem.ColumnHeaders[0].ExportType, Is.EqualTo(ExportValueType.String));
            Assert.That(exportedQuestionHeaderItem.ColumnHeaders[1].ExportType, Is.EqualTo(ExportValueType.String));
            Assert.That(exportedQuestionHeaderItem.ColumnHeaders[2].ExportType, Is.EqualTo(ExportValueType.String));
            Assert.That(exportedQuestionHeaderItem.ColumnHeaders[3].ExportType, Is.EqualTo(ExportValueType.String));

            Assert.That(exportedQuestionHeaderItem.QuestionSubType, Is.EqualTo(QuestionSubtype.MultiOptionLinkedNestedLevel));
            Assert.That(exportedQuestionHeaderItem.QuestionType, Is.EqualTo(QuestionType.MultyOption));
        }      
        

        [Test]
        public void when_creating_interview_export_view_by_interview_with_linked_multi_question_on_second_level_referenced_on_question_in_other_roster()
        {
            var questionnaire = Create.QuestionnaireDocumentWithOneChapter(
                Create.NumericIntegerQuestion(Id.g1),
                Create.Roster(Id.g2, rosterSizeQuestionId: Id.g1, rosterSizeSourceType:RosterSizeSourceType.Question, 
                    children: new IQuestionnaireEntity[]
                    {
                        Create.TextQuestion(Id.g3)
                    }),
                Create.MultyOptionsQuestion(Id.g8, areAnswersOrdered: true, options: new Answer[]
                {
                    new Answer() { AnswerValue = "1", AnswerText = "1"}, 
                    new Answer() { AnswerValue = "2", AnswerText = "2"}, 
                    new Answer() { AnswerValue = "3", AnswerText = "3"}, 
                }),
                Create.Roster(Id.g4, rosterSizeQuestionId: Id.g1, rosterSizeSourceType: RosterSizeSourceType.Question,
                    children: new IQuestionnaireEntity[]
                    {
                        Create.Roster(Id.g5, rosterSizeQuestionId: Id.g8, rosterSizeSourceType:RosterSizeSourceType.Question,
                            children: new IQuestionnaireEntity[]
                            {
                                Create.MultyOptionsQuestion(Id.g7, variable:"multi", linkedToQuestionId: Id.g3)
                            })
                    })
            );
            var questionnaireExportStructureFactory = CreateExportViewFactory();


            // act
            var questionnaireExportStructure = questionnaireExportStructureFactory.CreateQuestionnaireExportStructure(questionnaire);

            // assert
            HeaderStructureForLevel headerStructureForLevel = questionnaireExportStructure.HeaderToLevelMap[Create.ValueVector(Id.g1, Id.g8)];
            ExportedQuestionHeaderItem exportedQuestionHeaderItem = headerStructureForLevel.HeaderItems[Id.g7] as ExportedQuestionHeaderItem;

            Assert.That(exportedQuestionHeaderItem.ColumnHeaders.Count, Is.EqualTo(60));
            Assert.That(exportedQuestionHeaderItem.ColumnHeaders.Select(x => x.Name), Is.EqualTo(Enumerable.Range(0, 60).Select(value => "multi__" + value)));
            Assert.That(exportedQuestionHeaderItem.ColumnHeaders.Select(x => x.ExportType), Is.EqualTo(Enumerable.Range(0, 60).Select(_ => ExportValueType.NumericInt)));
            Assert.That(exportedQuestionHeaderItem.QuestionSubType, Is.EqualTo(QuestionSubtype.MultiOptionLinkedFirstLevel));
            Assert.That(exportedQuestionHeaderItem.QuestionType, Is.EqualTo(QuestionType.MultyOption));
        }

        [Test]
        public void when_combobox_with_max_answers_count_equal_2_then_columns_by_multioption_question_should_be_2()
        {
            // arrange
            var comboboxQuestionId = Guid.Parse("11111111111111111111111111111111");

            var questionnaire = Create.QuestionnaireDocumentWithOneChapter(
                Create.MultyOptionsQuestion(
                    id: comboboxQuestionId,
                    variable: "mult", 
                    isFilteredCombobox: true,
                    maxAnswersCount: 2,
                    options: new[] {Create.Answer("opt 1", 1), Create.Answer("opt 2", 2), Create.Answer("opt 3", 3)}));

            var QuestionnaireExportStructureFactory = CreateExportViewFactory();


            // act
            var questionnaireExportStructure = QuestionnaireExportStructureFactory.CreateQuestionnaireExportStructure(questionnaire);

            // assert
            HeaderStructureForLevel headerStructureForLevel = questionnaireExportStructure.HeaderToLevelMap[new ValueVector<Guid>()];
            ExportedQuestionHeaderItem exportedQuestionHeaderItem = headerStructureForLevel.HeaderItems[comboboxQuestionId] as ExportedQuestionHeaderItem;

            Assert.That(exportedQuestionHeaderItem.ColumnHeaders.Count, Is.EqualTo(2));
            Assert.That(exportedQuestionHeaderItem.ColumnHeaders.Select(x => x.Name).ToArray(), Is.EquivalentTo(new[] { "mult__0", "mult__1" }));
        }
        
        [Test]
        public void when_two_rosters_with_same_roster_source_then_roster_level_should_have_name_of_first_roster()
        {
            // arrange
            var questionnaire = Create.QuestionnaireDocumentWithOneChapter(new IQuestionnaireEntity[] {
                Create.NumericIntegerQuestion(Id.g1, variable: "int"),
                Create.Roster(rosterId: Id.g2, variable: "firstRoster", rosterSizeQuestionId: Id.g1, rosterSizeSourceType: RosterSizeSourceType.Question),
                Create.Roster(rosterId: Id.g3, variable: "secondRoster", rosterSizeQuestionId: Id.g1, rosterSizeSourceType: RosterSizeSourceType.Question),
            });

            var questionnaireExportStructureFactory = CreateExportViewFactory();


            // act
            var questionnaireExportStructure = questionnaireExportStructureFactory.CreateQuestionnaireExportStructure(questionnaire);

            // assert
            HeaderStructureForLevel rosterStructureForLevel = questionnaireExportStructure.HeaderToLevelMap[new ValueVector<Guid>(new[] { Id.g1 })];

            Assert.That(rosterStructureForLevel.LevelName, Is.EqualTo("firstRoster"));
        }
        
        [Test]
        public void when_two_rosters_with_same_roster_source_on_different_levels_then_roster_level_should_have_name_of_first_roster()
        {
            // arrange
            var questionnaire = Create.QuestionnaireDocumentWithOneChapter(new IQuestionnaireEntity[] {
                Create.NumericIntegerQuestion(Id.g1, variable: "int"),
                Create.Group(children: 
                    Create.Roster(rosterId: Id.g2, variable: "firstRoster", rosterSizeQuestionId: Id.g1, rosterSizeSourceType: RosterSizeSourceType.Question)
                ),
                Create.Roster(rosterId: Id.g3, variable: "secondRoster", rosterSizeQuestionId: Id.g1, rosterSizeSourceType: RosterSizeSourceType.Question),
            });

            var questionnaireExportStructureFactory = CreateExportViewFactory();


            // act
            var questionnaireExportStructure = questionnaireExportStructureFactory.CreateQuestionnaireExportStructure(questionnaire);

            // assert
            HeaderStructureForLevel rosterStructureForLevel = questionnaireExportStructure.HeaderToLevelMap[new ValueVector<Guid>(new[] { Id.g1 })];

            Assert.That(rosterStructureForLevel.LevelName, Is.EqualTo("firstRoster"));
        }

        [Test]
        public void when_exists_area_question_then_should_generate_all_columns()
        {
            // arrange
            var variable = "area";
            var questionnaire = Create.QuestionnaireDocumentWithOneChapter(new IQuestionnaireEntity[] {
                Create.AreaQuestion(Id.g1, variable: variable),
            });

            var questionnaireExportStructureFactory = CreateExportViewFactory();


            // act
            var questionnaireExportStructure = questionnaireExportStructureFactory.CreateQuestionnaireExportStructure(questionnaire);

            // assert
            HeaderStructureForLevel rosterStructureForLevel = questionnaireExportStructure.HeaderToLevelMap[new ValueVector<Guid>()];
            var areaColumnHeaders = rosterStructureForLevel.HeaderItems[Id.g1].ColumnHeaders;

            Assert.That(areaColumnHeaders[0].Name, Is.EqualTo(variable));
            Assert.That(areaColumnHeaders[1].Name, Is.EqualTo(variable + "__area"));
            Assert.That(areaColumnHeaders[2].Name, Is.EqualTo(variable + "__len"));
            Assert.That(areaColumnHeaders[3].Name, Is.EqualTo(variable + "__num"));
            Assert.That(areaColumnHeaders[4].Name, Is.EqualTo(variable + "__racc"));
        }

        [Test]
        public void when_exists_area_question_with_long_variable_then_should_generate_all_columns_with_short_name()
        {
            // arrange
            var variable = "area_variable_has_long_name_string";
            var questionnaire = Create.QuestionnaireDocumentWithOneChapter(new IQuestionnaireEntity[] {
                Create.AreaQuestion(Id.g1, variable: variable),
            });

            var questionnaireExportStructureFactory = CreateExportViewFactory();


            // act
            var questionnaireExportStructure = questionnaireExportStructureFactory.CreateQuestionnaireExportStructure(questionnaire);

            // assert
            HeaderStructureForLevel rosterStructureForLevel = questionnaireExportStructure.HeaderToLevelMap[new ValueVector<Guid>()];
            var areaColumnHeaders = rosterStructureForLevel.HeaderItems[Id.g1].ColumnHeaders;

            var shortVariable = "area_variable_has_long_nam0";
            Assert.That(areaColumnHeaders[0].Name.Length, Is.EqualTo(27));
            Assert.That(areaColumnHeaders[0].Name, Is.EqualTo(shortVariable));
            Assert.That(areaColumnHeaders[1].Name, Is.EqualTo(shortVariable + "__area"));
            Assert.That(areaColumnHeaders[2].Name, Is.EqualTo(shortVariable + "__len"));
            Assert.That(areaColumnHeaders[3].Name, Is.EqualTo(shortVariable + "__num"));
            Assert.That(areaColumnHeaders[4].Name, Is.EqualTo(shortVariable + "__racc"));
        }

        [Test]
        public void when_exists_area_question_with_26_chars_variable_then_should_generate_all_columns_with_short_name()
        {
            // arrange
            var variable = "area_variable_has_long_nam";
            var questionnaire = Create.QuestionnaireDocumentWithOneChapter(new IQuestionnaireEntity[] {
                Create.AreaQuestion(Id.g1, variable: variable),
            });

            var questionnaireExportStructureFactory = CreateExportViewFactory();


            // act
            var questionnaireExportStructure = questionnaireExportStructureFactory.CreateQuestionnaireExportStructure(questionnaire);

            // assert
            HeaderStructureForLevel rosterStructureForLevel = questionnaireExportStructure.HeaderToLevelMap[new ValueVector<Guid>()];
            var areaColumnHeaders = rosterStructureForLevel.HeaderItems[Id.g1].ColumnHeaders;

            Assert.That(areaColumnHeaders[0].Name.Length, Is.EqualTo(26));
            Assert.That(areaColumnHeaders[0].Name, Is.EqualTo(variable));
            Assert.That(areaColumnHeaders[1].Name, Is.EqualTo(variable + "__area"));
            Assert.That(areaColumnHeaders[2].Name, Is.EqualTo(variable + "__len"));
            Assert.That(areaColumnHeaders[3].Name, Is.EqualTo(variable + "__num"));
            Assert.That(areaColumnHeaders[4].Name, Is.EqualTo(variable + "__racc"));
        }

        [Test]
        public void when_exists_three_area_question_with_long_variables_then_should_generate_all_columns_with_short_name()
        {
            // arrange
            var variable1 = "area_variable_has_long_name_num1";
            var variable2 = "area_variable_has_long_name_num2";
            var variable3 = "area_variable_has_long_nam";
            var questionnaire = Create.QuestionnaireDocumentWithOneChapter(new IQuestionnaireEntity[] {
                Create.AreaQuestion(Id.g1, variable: variable1),
                Create.Group(children: new []
                {
                    Create.AreaQuestion(Id.g2, variable: variable2),
                    Create.AreaQuestion(Id.g3, variable: variable3),
                }),
            });

            var questionnaireExportStructureFactory = CreateExportViewFactory();


            // act
            var questionnaireExportStructure = questionnaireExportStructureFactory.CreateQuestionnaireExportStructure(questionnaire);

            // assert
            HeaderStructureForLevel rosterStructureForLevel = questionnaireExportStructure.HeaderToLevelMap[new ValueVector<Guid>()];
            
            var areaColumnHeaders1 = rosterStructureForLevel.HeaderItems[Id.g1].ColumnHeaders;
            var shortVariable1 = "area_variable_has_long_nam0";
            Assert.That(areaColumnHeaders1[0].Name.Length, Is.EqualTo(27));
            Assert.That(areaColumnHeaders1[0].Name, Is.EqualTo(shortVariable1));
            Assert.That(areaColumnHeaders1[1].Name, Is.EqualTo(shortVariable1 + "__area"));
            Assert.That(areaColumnHeaders1[2].Name, Is.EqualTo(shortVariable1 + "__len"));
            Assert.That(areaColumnHeaders1[3].Name, Is.EqualTo(shortVariable1 + "__num"));
            Assert.That(areaColumnHeaders1[4].Name, Is.EqualTo(shortVariable1 + "__racc"));

            var areaColumnHeaders2 = rosterStructureForLevel.HeaderItems[Id.g2].ColumnHeaders;
            var shortVariable2 = "area_variable_has_long_nam1";
            Assert.That(areaColumnHeaders2[0].Name.Length, Is.EqualTo(27));
            Assert.That(areaColumnHeaders2[0].Name, Is.EqualTo(shortVariable2));
            Assert.That(areaColumnHeaders2[1].Name, Is.EqualTo(shortVariable2 + "__area"));
            Assert.That(areaColumnHeaders2[2].Name, Is.EqualTo(shortVariable2 + "__len"));
            Assert.That(areaColumnHeaders2[3].Name, Is.EqualTo(shortVariable2 + "__num"));
            Assert.That(areaColumnHeaders2[4].Name, Is.EqualTo(shortVariable2 + "__racc"));
            
            var areaColumnHeaders3 = rosterStructureForLevel.HeaderItems[Id.g3].ColumnHeaders;
            var shortVariable3 = "area_variable_has_long_nam";
            Assert.That(areaColumnHeaders3[0].Name.Length, Is.EqualTo(26));
            Assert.That(areaColumnHeaders3[0].Name, Is.EqualTo(shortVariable3));
            Assert.That(areaColumnHeaders3[1].Name, Is.EqualTo(shortVariable3 + "__area"));
            Assert.That(areaColumnHeaders3[2].Name, Is.EqualTo(shortVariable3 + "__len"));
            Assert.That(areaColumnHeaders3[3].Name, Is.EqualTo(shortVariable3 + "__num"));
            Assert.That(areaColumnHeaders3[4].Name, Is.EqualTo(shortVariable3 + "__racc"));
        }
        
        [Test]
        public void when_exists_three_area_question_with__other_rosters_with_one_roster_trigger_should_generate_all_columns_with_short_name()
        {
            // arrange
            var variable1 = "area_variable_has_long_name_num1";
            var variable2 = "area_variable_has_long_name_num2";
            var variable3 = "area_variable_has_long_nam";
            var questionnaire = Create.QuestionnaireDocumentWithOneChapter(new IQuestionnaireEntity[] {
                Create.NumericIntegerQuestion(Id.gA),
                Create.Roster(Id.gB, rosterSizeQuestionId: Id.gA, children: new []
                {
                    Create.AreaQuestion(Id.g1, variable: variable1),
                }),
                Create.Roster(Id.gC, rosterSizeQuestionId: Id.gA, children: new []
                {
                    Create.Group(children: new []
                    {
                        Create.AreaQuestion(Id.g2, variable: variable2),
                        Create.AreaQuestion(Id.g3, variable: variable3),
                    }),
                }),
            });

            var questionnaireExportStructureFactory = CreateExportViewFactory();


            // act
            var questionnaireExportStructure = questionnaireExportStructureFactory.CreateQuestionnaireExportStructure(questionnaire);

            // assert
            HeaderStructureForLevel rosterStructureForLevel = questionnaireExportStructure.HeaderToLevelMap[new ValueVector<Guid>(new []{Id.gA})];
            
            var areaColumnHeaders1 = rosterStructureForLevel.HeaderItems[Id.g1].ColumnHeaders;
            var shortVariable1 = "area_variable_has_long_nam0";
            Assert.That(areaColumnHeaders1[0].Name.Length, Is.EqualTo(27));
            Assert.That(areaColumnHeaders1[0].Name, Is.EqualTo(shortVariable1));
            Assert.That(areaColumnHeaders1[1].Name, Is.EqualTo(shortVariable1 + "__area"));
            Assert.That(areaColumnHeaders1[2].Name, Is.EqualTo(shortVariable1 + "__len"));
            Assert.That(areaColumnHeaders1[3].Name, Is.EqualTo(shortVariable1 + "__num"));
            Assert.That(areaColumnHeaders1[4].Name, Is.EqualTo(shortVariable1 + "__racc"));

            var areaColumnHeaders2 = rosterStructureForLevel.HeaderItems[Id.g2].ColumnHeaders;
            var shortVariable2 = "area_variable_has_long_nam1";
            Assert.That(areaColumnHeaders2[0].Name.Length, Is.EqualTo(27));
            Assert.That(areaColumnHeaders2[0].Name, Is.EqualTo(shortVariable2));
            Assert.That(areaColumnHeaders2[1].Name, Is.EqualTo(shortVariable2 + "__area"));
            Assert.That(areaColumnHeaders2[2].Name, Is.EqualTo(shortVariable2 + "__len"));
            Assert.That(areaColumnHeaders2[3].Name, Is.EqualTo(shortVariable2 + "__num"));
            Assert.That(areaColumnHeaders2[4].Name, Is.EqualTo(shortVariable2 + "__racc"));
            
            var areaColumnHeaders3 = rosterStructureForLevel.HeaderItems[Id.g3].ColumnHeaders;
            var shortVariable3 = "area_variable_has_long_nam";
            Assert.That(areaColumnHeaders3[0].Name.Length, Is.EqualTo(26));
            Assert.That(areaColumnHeaders3[0].Name, Is.EqualTo(shortVariable3));
            Assert.That(areaColumnHeaders3[1].Name, Is.EqualTo(shortVariable3 + "__area"));
            Assert.That(areaColumnHeaders3[2].Name, Is.EqualTo(shortVariable3 + "__len"));
            Assert.That(areaColumnHeaders3[3].Name, Is.EqualTo(shortVariable3 + "__num"));
            Assert.That(areaColumnHeaders3[4].Name, Is.EqualTo(shortVariable3 + "__racc"));
        }

        [Test]
        public void when_exists_three_area_question_with_other_rosters_with_different_roster_trigger_should_generate_all_columns_with_short_name()
        {
            // arrange
            var variable1 = "area_variable_has_long_name_num1";
            var variable2 = "area_variable_has_long_name_num2";
            var variable3 = "area_variable_has_long_nam";
            var questionnaire = Create.QuestionnaireDocumentWithOneChapter(new IQuestionnaireEntity[] {
                Create.NumericIntegerQuestion(Id.gA),
                Create.Roster(Id.gB, rosterSizeQuestionId: Id.gA, children: new []
                {
                    Create.AreaQuestion(Id.g1, variable: variable1),
                }),
                Create.Roster(Id.gC, rosterSizeSourceType: RosterSizeSourceType.FixedTitles, children: new []
                {
                    Create.Group(children: new []
                    {
                        Create.AreaQuestion(Id.g2, variable: variable2),
                        Create.AreaQuestion(Id.g3, variable: variable3),
                    }),
                }),
            });

            var questionnaireExportStructureFactory = CreateExportViewFactory();


            // act
            var questionnaireExportStructure = questionnaireExportStructureFactory.CreateQuestionnaireExportStructure(questionnaire);

            // assert
            HeaderStructureForLevel rosterStructureForLevel = questionnaireExportStructure.HeaderToLevelMap[new ValueVector<Guid>(new []{Id.gA})];
            
            var areaColumnHeaders1 = rosterStructureForLevel.HeaderItems[Id.g1].ColumnHeaders;
            var shortVariable1 = "area_variable_has_long_nam0";
            Assert.That(areaColumnHeaders1[0].Name.Length, Is.EqualTo(27));
            Assert.That(areaColumnHeaders1[0].Name, Is.EqualTo(shortVariable1));
            Assert.That(areaColumnHeaders1[1].Name, Is.EqualTo(shortVariable1 + "__area"));
            Assert.That(areaColumnHeaders1[2].Name, Is.EqualTo(shortVariable1 + "__len"));
            Assert.That(areaColumnHeaders1[3].Name, Is.EqualTo(shortVariable1 + "__num"));
            Assert.That(areaColumnHeaders1[4].Name, Is.EqualTo(shortVariable1 + "__racc"));


            HeaderStructureForLevel rosterStructureForLevel2 = questionnaireExportStructure.HeaderToLevelMap[new ValueVector<Guid>(new []{Id.gC})];
            var areaColumnHeaders2 = rosterStructureForLevel2.HeaderItems[Id.g2].ColumnHeaders;
            var shortVariable2 = "area_variable_has_long_nam0";
            Assert.That(areaColumnHeaders2[0].Name.Length, Is.EqualTo(27));
            Assert.That(areaColumnHeaders2[0].Name, Is.EqualTo(shortVariable2));
            Assert.That(areaColumnHeaders2[1].Name, Is.EqualTo(shortVariable2 + "__area"));
            Assert.That(areaColumnHeaders2[2].Name, Is.EqualTo(shortVariable2 + "__len"));
            Assert.That(areaColumnHeaders2[3].Name, Is.EqualTo(shortVariable2 + "__num"));
            Assert.That(areaColumnHeaders2[4].Name, Is.EqualTo(shortVariable2 + "__racc"));
            
            var areaColumnHeaders3 = rosterStructureForLevel2.HeaderItems[Id.g3].ColumnHeaders;
            var shortVariable3 = "area_variable_has_long_nam";
            Assert.That(areaColumnHeaders3[0].Name.Length, Is.EqualTo(26));
            Assert.That(areaColumnHeaders3[0].Name, Is.EqualTo(shortVariable3));
            Assert.That(areaColumnHeaders3[1].Name, Is.EqualTo(shortVariable3 + "__area"));
            Assert.That(areaColumnHeaders3[2].Name, Is.EqualTo(shortVariable3 + "__len"));
            Assert.That(areaColumnHeaders3[3].Name, Is.EqualTo(shortVariable3 + "__num"));
            Assert.That(areaColumnHeaders3[4].Name, Is.EqualTo(shortVariable3 + "__racc"));
        }
    }
}
