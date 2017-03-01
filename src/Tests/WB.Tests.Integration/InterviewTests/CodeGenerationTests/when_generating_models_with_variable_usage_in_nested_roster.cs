﻿using System;
using System.Collections.Generic;
using System.Linq;
using AppDomainToolkit;
using Machine.Specifications;
using Main.Core.Documents;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using WB.Core.BoundedContexts.Designer.Implementation.Services.CodeGeneration;
using WB.Core.SharedKernels.DataCollection.V9;
using WB.Core.SharedKernels.QuestionnaireEntities;

namespace WB.Tests.Integration.InterviewTests.CodeGenerationTests
{
    internal class when_generating_models_with_variable_usage_in_nested_roster : CodeGenerationTestsContext
    {
        Establish context = () =>
        {
            appDomainContext = AppDomainContext.Create();
        };

        Because of = () =>
            results = Execute.InStandaloneAppDomain(appDomainContext.Domain, () =>
            {
                Guid questionnaireId = Guid.Parse("11111111111111111111111111111111");
                Guid variableId = Guid.Parse("11111111111111111111111111111112");
                Guid questionId = Guid.Parse("21111111111111111111111111111112");
                Guid rosterId = Guid.Parse("cccccccccccccccccccccccccccccccc");

                AssemblyContext.SetupServiceLocator();

                var assetsTitles = new[]
                {
                    Abc.Create.Entity.FixedTitle(1, "TV"),
                    Abc.Create.Entity.FixedTitle(2, "Microwave"),
                    Abc.Create.Entity.FixedTitle(3, "Cleaner")
                };

                var questionnaireDocument = Abc.Create.Entity.QuestionnaireDocumentWithOneChapter(questionnaireId, children: new IComposite[]
                {
                    Abc.Create.Entity.Group(children: new IComposite[]
                    {
                        Abc.Create.Entity.NumericIntegerQuestion(id: questionId, variable: "q1"),
                        Abc.Create.Entity.Variable(id: variableId, type: VariableType.Boolean, variableName: "num",
                            expression: "q1>5"),
                        Abc.Create.Entity.Roster(rosterId, variable: "assets",
                            rosterSizeSourceType: RosterSizeSourceType.FixedTitles, fixedRosterTitles: assetsTitles,
                            enablementCondition: "num.GetValueOrDefault()")
                    }),
                });
                IInterviewExpressionStateV9 state =
                    GetInterviewExpressionState(questionnaireDocument, version: 16) as
                        IInterviewExpressionStateV9;
                state.AddRoster(rosterId, new decimal[0], 1, null);
                state.AddRoster(rosterId, new decimal[0], 2, null);
                state.SaveAllCurrentStatesAsPrevious();
                var enablementConditions = state.ProcessEnablementConditions();

                return new InvokeResults()
                {
                    CoundOfDisabledGroups = enablementConditions.GroupsToBeDisabled.Count
                };
            });

        It should_disable_2_roster_groups = () =>
            results.CoundOfDisabledGroups.ShouldEqual(2);

        Cleanup stuff = () =>
        {
            appDomainContext.Dispose();
            appDomainContext = null;
        };

        private static AppDomainContext<AssemblyTargetLoader, PathBasedAssemblyResolver> appDomainContext;
        private static InvokeResults results;

        [Serializable]
        public class InvokeResults
        {
            public int CoundOfDisabledGroups { get; set; }
        }
    }
}