using System;
using System.Collections.Generic;
using Main.Core.Documents;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using Moq;
using WB.Core.BoundedContexts.Designer.Services;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.ChangeHistory;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.QuestionnaireEntities;
using WB.Core.SharedKernels.SurveySolutions.Documents;

namespace WB.Tests.Unit.Designer.BoundedContexts.Designer.QuestionnaireInfoFactoryTests
{
    internal class QuestionnaireInfoFactoryTestContext
    {
        protected static QuestionnaireInfoFactory CreateQuestionnaireInfoFactory(
            IDesignerQuestionnaireStorage questionDetailsReader = null,
            IExpressionProcessor expressionProcessor = null)
        {
            return new QuestionnaireInfoFactory(
                    questionDetailsReader ?? Mock.Of<IDesignerQuestionnaireStorage>(),
                    expressionProcessor ?? Mock.Of<IExpressionProcessor>());
        }

        protected static QuestionnaireDocument CreateQuestionnaireDocument()
        {
            return Create.QuestionnaireDocumentWithoutChildren(children: new IComposite[]
            {
                Create.Group(g1Id, title: "Group 1", children: new IComposite[]
                {
                    Create.Roster(rosterId:g2Id, title:"Roster 1.1", rosterSizeQuestionId: q2Id, rosterType: RosterSizeSourceType.Question, children: new IComposite[]
                    {
                        Create.Roster(rosterId:g3Id, title:"Roster 1.1.1", rosterSizeQuestionId: q2Id, fixedRosterTitles: new [] { new FixedRosterTitle(1, "1"), new FixedRosterTitle(2, "2"), new FixedRosterTitle(3, "3")}, children: new IComposite[]
                        {
                            Create.TextListQuestion(q4Id, title: "text list title"),
                            Create.NumericRealQuestion(q7Id, title: "numeric title"),
                        }),
                        Create.Group(g4Id, title: "Group 1.1.2", enablementCondition: "q1 > 40" ,children: new IComposite[]
                        {
                            Create.NumericRealQuestion(q5Id, title: "numeric title"),
                            Create.StaticText(st2Id, "static text 2"),
                        })
                    }),
                    Create.Roster(rosterId:g6Id, title:"Roster 1.2", rosterSizeQuestionId: numericQuestionId, rosterTitleQuestionId: q3Id, rosterType: RosterSizeSourceType.Question, children: new IComposite[]
                    {
                        Create.TextQuestion(q3Id, text: "text title"),
                    }),
                    Create.NumericIntegerQuestion(numericQuestionId, "q1", title: "Integer 1"),
                    Create.MultipleOptionsQuestion(q2Id, enablementCondition: "q1 > 25", title: "MultiOption", answersList: new List<Answer>{ Create.Option(1, "1"), Create.Option(2, "2") }),
                    Create.StaticText(st1Id, "static text 1"),
                    Create.Variable(var1Id, VariableType.String, "var1")
                }),
                Create.Group(g5Id, title: "Group 2", children: new IComposite[]
                {
                    Create.NumericIntegerQuestion(q6Id, "q6", title: "Integer 2"),
                    Create.MultimediaQuestion(q8Id, title: "Photo")
                }),
            });
        }

        protected static Guid g1Id = Guid.Parse("11111111111111111111111111111111");
        protected static Guid g2Id = Guid.Parse("22222222222222222222222222222222");
        protected static Guid g3Id = Guid.Parse("33333333333333333333333333333333");
        protected static Guid g4Id = Guid.Parse("44444444444444444444444444444444");
        protected static Guid g5Id = Guid.Parse("55555555555555555555555555555555");
        protected static Guid g6Id = Guid.Parse("66666666666666666666666666666666");

        protected static Guid q1Id = Guid.Parse("11111111111111111000000000000000");
        protected static Guid q2Id = Guid.Parse("22222222222222222000000000000000");
        protected static Guid q3Id = Guid.Parse("33333333333333333000000000000000");
        protected static Guid q4Id = Guid.Parse("44444444444444444000000000000000");
        protected static Guid q5Id = Guid.Parse("55555555555555555000000000000000");
        protected static Guid q6Id = Guid.Parse("66666666666666666000000000000000");
        protected static Guid q7Id = Guid.Parse("77777777777777777000000000000000");
        protected static Guid q8Id = Guid.Parse("88888888888888888000000000000000");
        protected static Guid q9Id = Guid.Parse("99999999999999999000000000000000");
        protected static Guid q10Id = Guid.Parse("10101010101010101000000000000000");
        protected static Guid numericQuestionId = Guid.NewGuid();

        protected static Guid st1Id = Guid.Parse("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC");
        protected static Guid st2Id = Guid.Parse("DDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDD");

        protected static Guid var1Id = Guid.Parse("11DDDDDDDDDDDDDDDDDDDDDDDDDDDDDD");
        protected static Guid docId = Guid.Parse("FFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFF");

        protected static QuestionnaireRevision questionnaireId = Create.QuestionnaireRevision("11111111111111111111111111111111");
    }
}
