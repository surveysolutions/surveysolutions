using System;
using System.Collections.Generic;
using Main.Core.Documents;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using Main.Core.Entities.SubEntities.Question;
using Moq;
using WB.Core.BoundedContexts.Designer.Services;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit.QuestionInfo;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.QuestionnaireEntities;
using WB.Core.SharedKernels.SurveySolutions.Documents;

namespace WB.Tests.Unit.Designer.BoundedContexts.Designer.QuestionnaireInfoFactoryTests
{
    internal class QuestionnaireInfoFactoryTestContext
    {
        protected static QuestionnaireInfoFactory CreateQuestionnaireInfoFactory(
            IPlainKeyValueStorage<QuestionnaireDocument> questionDetailsReader = null,
            IExpressionProcessor expressionProcessor = null)
        {
            return new QuestionnaireInfoFactory(
                    questionDetailsReader ?? Mock.Of<IPlainKeyValueStorage<QuestionnaireDocument>>(),
                    expressionProcessor ?? Mock.Of<IExpressionProcessor>());
        }

        protected static QuestionnaireDocument CreateQuestionnaireDocumentWithBrokenLinks()
        {
            return new QuestionnaireDocument
            {
                Children = new List<IComposite>()
                {
                    new Group()
                    {
                        PublicKey = g1Id,
                        Title = "Chapter 1",
                        Children = new List<IComposite>()
                        {
                            new Group()
                            {
                                PublicKey = multiOptionRoster,
                                Title = "Chapter 1 / Group 1",
                                Children = new List<IComposite>()
                                {
                                    new NumericQuestion()
                                    {
                                        PublicKey = numericQuestionId,
                                        IsInteger = true,
                                        QuestionText = "Integer 1",
                                        StataExportCaption = "q1",
                                        ConditionExpression = "q2 == \"aaaa\""
                                    },
                                    new SingleQuestion()
                                    {
                                        PublicKey = q5Id,
                                        QuestionText = "sINGLE 1",
                                        StataExportCaption = "qqqq",
                                    },
                                    new MultyOptionsQuestion()
                                    {
                                        PublicKey = q3Id,
                                        Answers = new List<Answer>()
                                        {
                                            new Answer() {AnswerText = "1", AnswerCode = 1},
                                            new Answer() {AnswerText = "2", AnswerCode = 2},
                                        },
                                        QuestionText = "MultiOption",
                                        ConditionExpression = "q2 == \"aaaa\""
                                    }
                                }
                            },
                            new Group()
                            {
                                PublicKey = fixedRoster,
                                Title = "Chapter 1 / Group 2",
                                Children = new List<IComposite>()
                                {
                                    new TextQuestion()
                                    {
                                        PublicKey = q2Id,
                                        QuestionText = "text title",
                                        ValidationConditions = new List<ValidationCondition> {new ValidationCondition { Expression = "q1 > 10" } }
                                    },
                                    new SingleQuestion()
                                    {
                                        PublicKey = q4Id,
                                        QuestionText = "single title",
                                        CascadeFromQuestionId = q5Id
                                    }
                                }
                            },
                        }
                    }
                }
            };
        }

      

        protected static QuestionnaireDocument CreateQuestionsAndGroupsCollectionView()
        {
            return new QuestionnaireDocument()
            {
                Children = new List<IComposite>()
                {
                    new Group()
                    {
                        PublicKey = g1Id,
                        Title = "Group 1",
                        Children = new List<IComposite>()
                        {
                            new Group()
                            {
                                PublicKey = multiOptionRoster,
                                Title = "Roster 1.1",
                                IsRoster = true,
                                RosterSizeSource = RosterSizeSourceType.Question,
                                RosterSizeQuestionId = q2Id,
                                RosterTitleQuestionId = null,
                                Children = new List<IComposite>()
                                {
                                    new Group()
                                    {
                                        PublicKey = fixedRoster,
                                        Title = "Roster 1.1.1",
                                        IsRoster = true,
                                        RosterSizeSource = RosterSizeSourceType.FixedTitles,
                                        FixedRosterTitles = new [] { new FixedRosterTitle(1, "1"), new FixedRosterTitle(2, "2"), new FixedRosterTitle(3, "3")},
                                        Children = new List<IComposite>()
                                        {
                                            Create.TextListQuestion(q4Id, title: "text list title"),
                                            Create.NumericRealQuestion(q7Id, title: "numeric title"),
                                        }
                                    },
                                    new Group()
                                    {
                                        PublicKey = g4Id,
                                        Title = "Group 1.1.2",
                                        ConditionExpression = "[" + numericQuestionId +"] > 40",
                                        Children = new List<IComposite>()
                                        {
                                            Create.NumericRealQuestion(q5Id, title: "numeric title"),
                                            Create.StaticText(st2Id, "static text 2"),
                                        }
                                    }
                                }
                            },
                            new Group()
                            {
                                PublicKey = numericRosterId,
                                Title = "Roster 1.2",
                                IsRoster = true,
                                RosterSizeSource = RosterSizeSourceType.Question,
                                RosterSizeQuestionId = numericQuestionId,
                                RosterTitleQuestionId = q3Id,
                                Children = new List<IComposite>()
                                {
                                    Create.TextQuestion(q3Id, text: "text title"),
                                }
                            },
                            Create.NumericIntegerQuestion(numericQuestionId, "q1", title: "Integer 1"),
                            Create.MultipleOptionsQuestion(q2Id, "["+ numericQuestionId +"] > 25", title: "MultiOption",
                                answersList: new List<Answer>() 
                                {
                                    new Answer() {AnswerText = "1", AnswerCode = 1},
                                    new Answer() {AnswerText = "2", AnswerCode = 2},
                                }),
                            Create.StaticText(st1Id, "static text 1")
                        }
                    },
                    new Group()
                    {
                        PublicKey = g5Id,
                        Title = "Group 2",
                        Children = new List<IComposite>()
                        {
                            new NumericQuestion()
                            {
                                PublicKey = q6Id,
                                QuestionText = "Integer 2",
                                IsInteger = true,
                            },
                            new MultimediaQuestion()
                            {
                                PublicKey = q8Id,
                                QuestionText = "Photo",
                            }
                        }
                    }
                }
            };
        }

        protected static QuestionnaireDocument CreateRosterWithNoTrigger()
        {
            return new QuestionnaireDocument()
            {
                Children = new List<IComposite>()
                {
                    new Group()
                    {
                        PublicKey = g1Id,
                        Title = "Group 1",
                        Children = new List<IComposite>()
                        {
                            new Group()
                            {
                                PublicKey = multiOptionRoster,
                                Title = "Roster 1.1",
                                IsRoster = true,
                                RosterSizeSource = RosterSizeSourceType.Question,
                                RosterSizeQuestionId = q2Id,
                                RosterTitleQuestionId = q3Id,
                            }
                        }
                    }
                }
            };
        }

        protected static QuestionnaireDocument CreateQuestionsAndGroupsCollectionViewWithCascadingQuestions()
        {
            return new QuestionnaireDocument()
            {
                Children = new List<IComposite>()
                {
                    new Group()
                    {
                        PublicKey = g1Id,
                        Title = "Group 1",
                        Children = new List<IComposite>()
                        {
                            new Group()
                            {
                                PublicKey = multiOptionRoster,
                                Title = "Roster",
                                IsRoster = true,
                                RosterSizeSource = RosterSizeSourceType.FixedTitles,
                                FixedRosterTitles = new [] { new FixedRosterTitle(1, "1"), new FixedRosterTitle(2, "2"), new FixedRosterTitle(3, "3")},
                                Children = new List<IComposite>()
                                {
                                    Create.NumericIntegerQuestion(q4Id, "int",  title:"Integer 1"),
                                    Create.SingleQuestion(q5Id, "linked_question", linkedToQuestionId:q4Id, title:"linked"),
                                }
                            },
                            Create.SingleQuestion(q1Id, "list_question", title: "cascading_question"),
                            Create.SingleQuestion(q2Id, "list_question", title: "cascading_question_2", cascadeFromQuestionId: q1Id),
                            Create.SingleQuestion(q3Id, "list_question", title: "cascading_question_3", cascadeFromQuestionId: q2Id),
                        }
                    }
                }
            };
        }

        protected static QuestionnaireDocument CreateQuestionsAndGroupsCollectionViewWithListQuestions(bool shouldReplaceFixedRosterWithListOne = false)
        {
            var fixedNestedRoster = new Group
            {
                PublicKey = fixedRoster,
                Title = "fixed_roster_inside_list_roster",
                VariableName = "fixed_roster_inside_list_roster",
                IsRoster = true,
                RosterSizeSource = RosterSizeSourceType.FixedTitles,
                FixedRosterTitles = new[] { new FixedRosterTitle(1, "1"), new FixedRosterTitle(2, "2"), new FixedRosterTitle(3, "3") },
                Children = new List<IComposite>()
                {
                     Create.TextListQuestion(q3Id, variable:"list_question", title: "list_question_inside_fixed_roster", maxAnswerCount: 16),
                }
            };
            var listNestedRoster = new Group
            {
                PublicKey = fixedRoster,
                Title = "list_roster_inside_list_roster",
                VariableName = "fixed_roster_inside_list_roster",
                IsRoster = true,
                RosterSizeSource = RosterSizeSourceType.Question,
                RosterSizeQuestionId = q2Id,
                Children = new List<IComposite>()
                {
                     Create.TextListQuestion(q3Id, variable:"list_question", title: "list_question_inside_fixed_roster", maxAnswerCount: 16),
                }
            };
            return new QuestionnaireDocument()
            {
                Children = new List<IComposite>()
                {
                    new Group()
                    {
                        PublicKey = g1Id,
                        Title = "Chapter",
                        Children = new List<IComposite>()
                        {
                            new Group()
                            {
                                PublicKey = multiOptionRoster,
                                Title = "list_roster",
                                VariableName = "list_roster",
                                IsRoster = true,
                                RosterSizeSource = RosterSizeSourceType.Question,
                                RosterSizeQuestionId = q1Id,
                                Children = new List<IComposite>()
                                {
                                    (shouldReplaceFixedRosterWithListOne? listNestedRoster : fixedNestedRoster),

                                    Create.TextListQuestion(q2Id, variable:"list_question", title: "list_question_inside_roster", maxAnswerCount: 16),
                                }
                            },
                            Create.TextListQuestion(q1Id, variable:"list_question", title: "list_question"),
                        }
                    }
                }
            };
        }

        protected static Guid g1Id = Guid.Parse("11111111111111111111111111111111");
        protected static Guid multiOptionRoster = Guid.Parse("22222222222222222222222222222222");
        protected static Guid fixedRoster = Guid.Parse("33333333333333333333333333333333");
        protected static Guid g4Id = Guid.Parse("44444444444444444444444444444444");
        protected static Guid g5Id = Guid.Parse("55555555555555555555555555555555");
        protected static Guid numericRosterId = Guid.NewGuid();

        protected static Guid q1Id = Guid.Parse("66666666666666666666666666666666");
        protected static Guid q2Id = Guid.Parse("77777777777777777777777777777777");
        protected static Guid q3Id = Guid.Parse("88888888888888888888888888888888");
        protected static Guid q4Id = Guid.Parse("99999999999999999999999999999999");
        protected static Guid q5Id = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
        protected static Guid q6Id = Guid.Parse("BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB");
        protected static Guid q7Id = Guid.Parse("EEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEE");
        protected static Guid q8Id = Guid.Parse("11EEEEEEEEEEEEEEEEEEEEEEEEEEEEEE");
        protected static Guid numericQuestionId = Guid.NewGuid();


        protected static Guid st1Id = Guid.Parse("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC");
        protected static Guid st2Id = Guid.Parse("DDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDD");

    }
}
