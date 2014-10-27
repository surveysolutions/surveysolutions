using System;
using System.Collections.Generic;
using Machine.Specifications;
using WB.Core.BoundedContexts.Designer.Implementation.Services.CodeGeneration.Model;

namespace WB.Core.BoundedContexts.Designer.Tests.CodeGenerationModelTests
{
    internal class when_getting_ordered_list_by_condition_dependency : CodeGenerationModelTestsContext
    {
        Establish context = () =>
        {
            var conditionalDependencies = new Dictionary<Guid, List<Guid>>
            {
                { group1Id, new List<Guid>() { question2Id } },
                { group2Id, new List<Guid>() { question2Id, question1Id } },
                { question2Id, new List<Guid>() { question1Id } },
                { roster1Id, new List<Guid>() { question2Id }}
            };

            questions = new List<QuestionTemplateModel>()
            {
                new QuestionTemplateModel()
                {
                    Id = question2Id,
                    Conditions = "dummy",
                    GeneratedConditionsMethodName = question2GeneratedName,
                    GeneratedStateName = question2GeneratedName
                }
            };

            groups = new List<GroupTemplateModel>()
            {
                new GroupTemplateModel()
                {
                  Id = group1Id,
                  Conditions = "dummy",
                  GeneratedConditionsMethodName = group1GeneratedName,
                  GeneratedStateName = group1GeneratedName
                }
            };

            rosters = new List<RosterTemplateModel>()
            {
                new RosterTemplateModel()
                {
                  Id = roster1Id,
                  Conditions = "dummy",
                  GeneratedConditionsMethodName = roster1GeneratedName,
                  GeneratedStateName = roster1GeneratedName
                }
            };

            questionnaireExecutorTemplateModel = CreateQuestionnaireExecutorTemplateModel(conditionalDependencies);
            
        };

        Because of = () =>
            result = questionnaireExecutorTemplateModel.GetOrderedListByConditionDependency(questions, groups, rosters);

        It should_ = () =>
            result.Count.ShouldEqual(3);

        It should_be = () =>
            result[0].Item1.ShouldEqual(question2GeneratedName);

        It should_be2 = () =>
            result[1].Item1.ShouldEqual(group1GeneratedName);

        It should_be3 = () =>
            result[2].Item1.ShouldEqual(roster1GeneratedName);

        private static string question1GeneratedName = "11111111111111111111111111111111";

        private static Guid question1Id = Guid.Parse(question1GeneratedName);

        private static string question2GeneratedName = "11111111111111111111111111111112";
        private static Guid question2Id = Guid.Parse(question2GeneratedName);

        private static string group1GeneratedName = "31111111111111111111111111111111";
        private static Guid group1Id = Guid.Parse(group1GeneratedName);

        private static string group2GeneratedName = "31111111111111111111111111111112";
        private static Guid group2Id = Guid.Parse(group2GeneratedName);

        private static string roster1GeneratedName = "51111111111111111111111111111111";
        private static Guid roster1Id = Guid.Parse(roster1GeneratedName);

        private static QuestionnaireExecutorTemplateModel questionnaireExecutorTemplateModel;
        private static List<Tuple<string, string>> result;

        private static List<QuestionTemplateModel> questions;
        private static List<GroupTemplateModel> groups;
        private static List<RosterTemplateModel> rosters;
    }
}
