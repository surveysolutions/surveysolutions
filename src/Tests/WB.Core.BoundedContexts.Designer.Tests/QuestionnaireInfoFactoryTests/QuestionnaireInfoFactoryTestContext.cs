using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Main.Core.Entities.SubEntities;
using Moq;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit.QuestionInfo;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;

namespace WB.Core.BoundedContexts.Designer.Tests.QuestionnaireInfoFactoryTests
{
    internal class QuestionnaireInfoFactoryTestContext
    {
        protected static QuestionnaireInfoFactory CreateQuestionnaireInfoFactory(IReadSideRepositoryReader<QuestionsAndGroupsCollectionView> questionDetailsReader = null)
        {
            return new QuestionnaireInfoFactory(questionDetailsReader ?? Mock.Of<IReadSideRepositoryReader<QuestionsAndGroupsCollectionView>>());
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
                        RosterFixedTitles = new[] { "1", "2", "3" },
                        ParentGroupId = g2Id,
                        ParentGroupsIds = new Guid[] { g2Id, g1Id },
                        RosterScopeIds = new Guid[] { g3Id, q2Id }
                    },
                    new GroupAndRosterDetailsView
                    {
                        Id = g4Id,
                        Title = "Group 1.1.2",
                        Description = "Group 1.1.2 Description",
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
                        ParentGroupsIds = new Guid[] { g2Id, g1Id },
                        RosterScopeIds = new Guid[] { q2Id }
                    },
                    new TextListDetailsView
                    {
                        Id = q4Id,
                        ParentGroupId = g3Id,
                        ParentGroupsIds = new Guid[] { g3Id, g2Id, g1Id },
                        RosterScopeIds = new Guid[] { g3Id, q2Id }
                    },
                    new NumericDetailsView
                    {
                        Id = q5Id,
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
                    }
                },
                StaticTexts = new List<StaticTextDetailsView>()
                {
                    new StaticTextDetailsView()
                    {
                        Id = st1Id,
                        ParentGroupId = g1Id,
                        ParentGroupsIds = new [] { g1Id },
                        RosterScopeIds = new Guid[] { },
                        Text = "static text 1"
                    },
                    new StaticTextDetailsView()
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

        protected static Guid st1Id = Guid.Parse("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC");
        protected static Guid st2Id = Guid.Parse("DDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDD");
    }
}
