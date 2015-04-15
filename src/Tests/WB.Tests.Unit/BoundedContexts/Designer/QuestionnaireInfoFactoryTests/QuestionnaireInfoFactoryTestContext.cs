using System;
using System.Collections.Generic;
using Main.Core.Entities.SubEntities;
using Moq;

using WB.Core.BoundedContexts.Designer.Services;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit.QuestionInfo;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;

namespace WB.Tests.Unit.BoundedContexts.Designer.QuestionnaireInfoFactoryTests
{
    internal class QuestionnaireInfoFactoryTestContext
    {
        protected static QuestionnaireInfoFactory CreateQuestionnaireInfoFactory(
            IReadSideKeyValueStorage<QuestionsAndGroupsCollectionView> questionDetailsReader = null,
            IExpressionProcessor expressionProcessor = null)
        {
            return new QuestionnaireInfoFactory(
                    questionDetailsReader ?? Mock.Of<IReadSideKeyValueStorage<QuestionsAndGroupsCollectionView>>(),
                    expressionProcessor ?? Mock.Of<IExpressionProcessor>());
        }

        protected static QuestionsAndGroupsCollectionView CreateQuestionsAndGroupsCollectionViewWithBrokenLinks()
        {
            return new QuestionsAndGroupsCollectionView
            {
                Groups = new List<GroupAndRosterDetailsView>
                {
                    new GroupAndRosterDetailsView
                    {
                        Id = g1Id,
                        Title = "Chapter 1",
                        ParentGroupId = Guid.Empty,
                        ParentGroupsIds = new Guid[0],
                        RosterScopeIds = new Guid[0]
                    },
                    new GroupAndRosterDetailsView
                    {
                        Id = g2Id,
                        Title = "Chapter 1 / Group 1",
                        ParentGroupId = g1Id,
                        ParentGroupsIds = new[] { g1Id }
                    },
                    new GroupAndRosterDetailsView
                    {
                        Id = g3Id,
                        Title = "Chapter 1/ Group 2",
                        ParentGroupId = g1Id,
                        ParentGroupsIds =  new[] { g1Id }
                    }
                },
                Questions = new List<QuestionDetailsView>
                {
                    new NumericDetailsView
                    {
                        Id = q1Id,
                        IsInteger = true,
                        Title = "Integer 1",
                        MaxValue = 20,
                        ParentGroupId = g2Id,
                        VariableName = "q1",
                        ParentGroupsIds = new [] { g2Id, g1Id },
                        RosterScopeIds = new Guid[] {  },
                        EnablementCondition = "q2 == \"aaaa\""
                    },
                    new TextDetailsView
                    {
                        Id = q2Id,
                        ParentGroupId = g3Id,
                        Title = "text title",
                        ParentGroupsIds = new [] { g3Id, g1Id },
                        RosterScopeIds = new Guid[] {  },
                        ValidationExpression = "q1 > 10"
                    },
                     new MultiOptionDetailsView
                    {
                        Id = q3Id,
                        Options = new CategoricalOption[]
                        {
                            new CategoricalOption {Title = "1", Value = 1},
                            new CategoricalOption {Title = "2", Value = 2}
                        },
                        Title = "MultiOption",
                        ParentGroupId = g2Id,
                        ParentGroupsIds = new [] { g2Id, g1Id },
                        RosterScopeIds = new Guid[] {  },
                        EnablementCondition = "q2 == \"aaaa\""
                    },
                },
                StaticTexts = new List<StaticTextDetailsView>()
            };
        }

      

        protected static QuestionsAndGroupsCollectionView CreateQuestionsAndGroupsCollectionView()
        {
            return new QuestionsAndGroupsCollectionView
            {
                Groups = new List<GroupAndRosterDetailsView>
                {
                    new GroupAndRosterDetailsView
                    {
                        Id = g1Id,
                        Title = "Group 1",
                        ParentGroupId = Guid.Empty,
                        ParentGroupsIds = new Guid[0],
                        RosterScopeIds = new Guid[0]
                    },
                    new GroupAndRosterDetailsView
                    {
                        Id = g2Id,
                        Title = "Roster 1.1",
                        IsRoster = true,
                        RosterSizeSourceType = RosterSizeSourceType.Question,
                        RosterSizeQuestionId = q2Id,
                        RosterTitleQuestionId = q3Id,
                        ParentGroupId = g1Id,
                        ParentGroupsIds = new Guid[] { g1Id },
                        RosterScopeIds = new Guid[] { q2Id }
                    },
                    new GroupAndRosterDetailsView
                    {
                        Id = g3Id,
                        Title = "Roster 1.1.1",
                        IsRoster = true,
                        RosterSizeSourceType = RosterSizeSourceType.FixedTitles,
                        FixedRosterTitles = new Dictionary<decimal, string>() { {1,"1"}, {2,"2"}, {3,"3"} },
                        ParentGroupId = g2Id,
                        ParentGroupsIds = new Guid[] { g2Id, g1Id },
                        RosterScopeIds = new Guid[] { g3Id, q2Id }
                    },
                    new GroupAndRosterDetailsView
                    {
                        Id = g4Id,
                        Title = "Group 1.1.2",
                        EnablementCondition = "[" + q1Id +"] > 40",
                        ParentGroupId = g2Id,
                        ParentGroupsIds = new Guid[] { g2Id, g1Id },
                        RosterScopeIds = new Guid[] { q2Id }
                    },
                    new GroupAndRosterDetailsView
                    {
                        Id = g5Id,
                        Title = "Group 2",
                        ParentGroupId = Guid.Empty,
                        ParentGroupsIds = new Guid[0]
                    }
                },
                Questions = new List<QuestionDetailsView>
                {
                    new NumericDetailsView
                    {
                        Id = q1Id,
                        IsInteger = true,
                        Title = "Integer 1",
                        MaxValue = 20,
                        ParentGroupId = g1Id,
                        VariableName = "q1",
                        ParentGroupsIds = new Guid[] { g1Id },
                        RosterScopeIds = new Guid[] {  }
                    },
                    new MultiOptionDetailsView
                    {
                        Id = q2Id,
                        Options = new CategoricalOption[]
                        {
                            new CategoricalOption {Title = "1", Value = 1},
                            new CategoricalOption {Title = "2", Value = 2}
                        },
                        Title = "MultiOption",
                        ParentGroupId = g1Id,
                        ParentGroupsIds = new Guid[] { g1Id },
                        RosterScopeIds = new Guid[] {  },
                        EnablementCondition = "["+ q1Id +"] > 25"
                    },
                    new TextDetailsView
                    {
                        Id = q3Id,
                        ParentGroupId = g2Id,
                        Title = "text title",
                        ParentGroupsIds = new Guid[] { g2Id, g1Id },
                        RosterScopeIds = new Guid[] { q2Id }
                    },
                    new TextListDetailsView
                    {
                        Id = q4Id,
                        Title = "text list title",
                        ParentGroupId = g3Id,
                        ParentGroupsIds = new Guid[] { g3Id, g2Id, g1Id },
                        RosterScopeIds = new Guid[] { g3Id, q2Id }
                    },
                    new NumericDetailsView
                    {
                        Id = q7Id,
                        Title = "numeric title",
                        ParentGroupId = g3Id,
                        ParentGroupsIds = new Guid[] { g3Id, g2Id, g1Id },
                        RosterScopeIds = new Guid[] { g3Id, q2Id }
                    },
                    new NumericDetailsView
                    {
                        Id = q5Id,
                        Title = "numeric title",
                        IsInteger = false,
                        ParentGroupId = g4Id,
                        ParentGroupsIds = new Guid[] { g4Id, g2Id, g1Id },
                        RosterScopeIds = new Guid[] { q2Id }
                    },
                    new NumericDetailsView
                    {
                        Id = q6Id,
                        Title = "Integer 2",
                        IsInteger = true,
                        ParentGroupId = g5Id,
                        ParentGroupsIds = new Guid[] { g5Id },
                        RosterScopeIds = new Guid[] {  }
                    },
                    new MultimediaDetailsView
                    {
                        Id = q8Id,
                        Title = "Photo",
                        ParentGroupId = g5Id,
                        ParentGroupsIds = new Guid[] { g5Id },
                        RosterScopeIds = new Guid[] {  }
                    }
                },
                StaticTexts = new List<StaticTextDetailsView>()
                {
                    new StaticTextDetailsView
                    {
                        Id = st1Id,
                        ParentGroupId = g1Id,
                        ParentGroupsIds = new [] { g1Id },
                        RosterScopeIds = new Guid[] { },
                        Text = "static text 1"
                    },
                    new StaticTextDetailsView
                    {
                        Id = st2Id,
                        ParentGroupId = g4Id,
                        ParentGroupsIds = new Guid[] { g4Id, g2Id, g1Id },
                        RosterScopeIds = new Guid[] { q2Id },
                        Text = "static text 2"
                    }
                }
            };
        }

        protected static QuestionsAndGroupsCollectionView CreateQuestionsAndGroupsCollectionViewWithCascadingQuestions()
        {
            return new QuestionsAndGroupsCollectionView
            {
                Groups = new List<GroupAndRosterDetailsView>
                {
                    new GroupAndRosterDetailsView
                    {
                        Id = g1Id,
                        Title = "Chapter",
                        ParentGroupId = Guid.Empty,
                        ParentGroupsIds = new Guid[0],
                        RosterScopeIds = new Guid[0]
                    },
                    new GroupAndRosterDetailsView
                    {
                        Id = g2Id,
                        Title = "Roster",
                        IsRoster = true,
                        RosterSizeSourceType = RosterSizeSourceType.FixedTitles,
                        FixedRosterTitles = new Dictionary<decimal, string>() { {1,"1"}, {2,"2"}, {3,"3"} },
                        ParentGroupId = g1Id,
                        ParentGroupsIds = new Guid[] { g1Id },
                        RosterScopeIds = new Guid[] { q2Id }
                    }
                },
                Questions = new List<QuestionDetailsView>
                {
                    new SingleOptionDetailsView
                    {
                        Id = q1Id,
                        Title = "cascading_question",
                        ParentGroupId = g1Id,
                        VariableName = "list_question",
                        ParentGroupsIds = new Guid[] { g1Id },
                        RosterScopeIds = new Guid[] {  }
                    },
                    new SingleOptionDetailsView
                    {
                        Id = q2Id,
                        Title = "cascading_question_2",
                        ParentGroupId = g1Id,
                        VariableName = "list_question",
                        ParentGroupsIds = new Guid[] { g1Id },
                        RosterScopeIds = new Guid[] {  },
                        CascadeFromQuestionId = q1Id
                    },
                    new SingleOptionDetailsView
                    {
                        Id = q3Id,
                        Title = "cascading_question_3",
                        ParentGroupId = g1Id,
                        VariableName = "list_question",
                        ParentGroupsIds = new Guid[] {  g1Id },
                        RosterScopeIds = new Guid[] { },
                        CascadeFromQuestionId = q2Id
                    },
                    new NumericDetailsView
                    {
                        Id = q4Id,
                        IsInteger = true,
                        Title = "Integer 1",
                        MaxValue = 20,
                        ParentGroupId = g2Id,
                        VariableName = "int",
                        ParentGroupsIds = new Guid[] { g2Id },
                        RosterScopeIds = new Guid[] {  g2Id }
                    },
                    new SingleOptionDetailsView
                    {
                        Id = q5Id,
                        Title = "linked",
                        ParentGroupId = g2Id,
                        VariableName = "linked_question",
                        ParentGroupsIds = new Guid[] { g2Id },
                        RosterScopeIds = new Guid[] { g2Id },
                        LinkedToQuestionId = q4Id
                    },
                },
                StaticTexts = new List<StaticTextDetailsView>()
            };
        }

        protected static QuestionsAndGroupsCollectionView CreateQuestionsAndGroupsCollectionViewWithListQuestions(bool shouldReplaceFixedRosterWithListOne = false)
        {
            var fixedNestedRoster = new GroupAndRosterDetailsView
            {
                Id = g3Id,
                Title = "fixed_roster_inside_list_roster",
                VariableName = "fixed_roster_inside_list_roster",
                IsRoster = true,
                RosterSizeSourceType = RosterSizeSourceType.FixedTitles,
                FixedRosterTitles = new Dictionary<decimal, string>() { {1, "1"}, {2, "2"}, {3, "3"} },
                ParentGroupId = g2Id,
                ParentGroupsIds = new Guid[] { g2Id, g1Id },
                RosterScopeIds = new Guid[] { g3Id, q1Id }
            };
            var listNestedRoster = new GroupAndRosterDetailsView
            {
                Id = g3Id,
                Title = "list_roster_inside_list_roster",
                VariableName = "fixed_roster_inside_list_roster",
                IsRoster = true,
                RosterSizeSourceType = RosterSizeSourceType.Question,
                RosterSizeQuestionId = q2Id,
                ParentGroupId = g2Id,
                ParentGroupsIds = new Guid[] { g2Id, g1Id },
                RosterScopeIds = new Guid[] { q2Id, q1Id }
            };
            return new QuestionsAndGroupsCollectionView
            {
                Groups = new List<GroupAndRosterDetailsView>
                {
                    new GroupAndRosterDetailsView
                    {
                        Id = g1Id,
                        Title = "Chapter",
                        ParentGroupId = Guid.Empty,
                        ParentGroupsIds = new Guid[0],
                        RosterScopeIds = new Guid[0]
                    },
                    new GroupAndRosterDetailsView
                    {
                        Id = g2Id,
                        Title = "list_roster",
                        VariableName = "list_roster",
                        IsRoster = true,
                        RosterSizeSourceType = RosterSizeSourceType.Question,
                        RosterSizeQuestionId = q1Id,
                        ParentGroupId = g1Id,
                        ParentGroupsIds = new Guid[] { g1Id },
                        RosterScopeIds = new Guid[] { q1Id }
                    },
                    (shouldReplaceFixedRosterWithListOne? listNestedRoster : fixedNestedRoster)
                },
                Questions = new List<QuestionDetailsView>
                {
                    new TextListDetailsView
                    {
                        Id = q1Id,
                        Title = "list_question",
                        MaxAnswerCount = 16,
                        ParentGroupId = g1Id,
                        VariableName = "list_question",
                        ParentGroupsIds = new Guid[] { g1Id },
                        RosterScopeIds = new Guid[] {  }
                    },
                    new TextListDetailsView
                    {
                        Id = q2Id,
                        Title = "list_question_inside_roster",
                        MaxAnswerCount = 16,
                        ParentGroupId = g1Id,
                        VariableName = "list_question",
                        ParentGroupsIds = new Guid[] { g2Id, g1Id },
                        RosterScopeIds = new Guid[] { q1Id }
                    },
                    new TextListDetailsView
                    {
                        Id = q3Id,
                        Title = "list_question_inside_fixed_roster",
                        MaxAnswerCount = 16,
                        ParentGroupId = g1Id,
                        VariableName = "list_question",
                        ParentGroupsIds = new Guid[] { g3Id, g2Id, g1Id },
                        RosterScopeIds = new Guid[] { g3Id, q1Id }
                    }
                },
                StaticTexts = new List<StaticTextDetailsView>()
            };
        }

        protected static Guid g1Id = Guid.Parse("11111111111111111111111111111111");
        protected static Guid g2Id = Guid.Parse("22222222222222222222222222222222");
        protected static Guid g3Id = Guid.Parse("33333333333333333333333333333333");
        protected static Guid g4Id = Guid.Parse("44444444444444444444444444444444");
        protected static Guid g5Id = Guid.Parse("55555555555555555555555555555555");

        protected static Guid q1Id = Guid.Parse("66666666666666666666666666666666");
        protected static Guid q2Id = Guid.Parse("77777777777777777777777777777777");
        protected static Guid q3Id = Guid.Parse("88888888888888888888888888888888");
        protected static Guid q4Id = Guid.Parse("99999999999999999999999999999999");
        protected static Guid q5Id = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
        protected static Guid q6Id = Guid.Parse("BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB");
        protected static Guid q7Id = Guid.Parse("EEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEE");
        protected static Guid q8Id = Guid.Parse("11EEEEEEEEEEEEEEEEEEEEEEEEEEEEEE");
        
        protected static Guid st1Id = Guid.Parse("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC");
        protected static Guid st2Id = Guid.Parse("DDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDD");
    }
}
