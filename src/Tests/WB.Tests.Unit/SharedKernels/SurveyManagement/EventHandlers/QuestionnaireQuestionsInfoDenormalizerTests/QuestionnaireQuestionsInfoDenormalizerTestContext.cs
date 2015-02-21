extern alias datacollection;

using System;
using System.Collections.Generic;
using Main.Core.Documents;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using Main.Core.Entities.SubEntities.Question;
using Main.Core.Events.Questionnaire;
using Moq;
using Ncqrs.Eventing.ServiceModel.Bus;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection.Events.Questionnaire;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.SurveyManagement.EventHandler;
using WB.Core.SharedKernels.SurveyManagement.Views.Questionnaire;

using QuestionnaireDeleted = WB.Core.SharedKernels.DataCollection.Events.Questionnaire.QuestionnaireDeleted;
using TemplateImported = datacollection::Main.Core.Events.Questionnaire.TemplateImported;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.EventHandlers.QuestionnaireQuestionsInfoDenormalizerTests
{
    internal class QuestionnaireQuestionsInfoDenormalizerTestContext
    {
        protected static IPublishedEvent<T> ToPublishedEvent<T>(T @event, Guid? eventSourceId = null) where T : class
        {
            return Mock.Of<IPublishedEvent<T>>(publishedEvent => publishedEvent.Payload == @event 
                && publishedEvent.EventSourceId == (eventSourceId ?? Guid.Parse("33333333333333333333333333333333"))
                && publishedEvent.EventSequence == 1);
        }

        protected static IPublishedEvent<TemplateImported> CreateTemplateImportedEvent(QuestionnaireDocument questionnaire = null, long? version=null)
        {
            var questionnaireDocument = questionnaire ?? new QuestionnaireDocument();
            return ToPublishedEvent(new TemplateImported
            {
                Source = questionnaireDocument,
                Version = version
            }, questionnaireDocument.PublicKey);
        }

        protected static IPublishedEvent<QuestionnaireDeleted> CreateQuestionnaireDeletedEvent(Guid? questionnaireId = null,  long? version=null)
        {
            questionnaireId = questionnaireId ?? Guid.NewGuid();
            return ToPublishedEvent(new QuestionnaireDeleted
            {
                QuestionnaireVersion = version ?? 1
            }, questionnaireId);
        }

        protected static QuestionnaireQuestionsInfoDenormalizer CreateQuestionnaireQuestionsInfoDenormalizer(IReadSideKeyValueStorage<QuestionnaireQuestionsInfo> questionnaires = null)
        {
            return new QuestionnaireQuestionsInfoDenormalizer(
                questionnaires ?? Mock.Of<IReadSideKeyValueStorage<QuestionnaireQuestionsInfo>>(),
                Mock.Of<IPlainQuestionnaireRepository>());
        }

        protected static QuestionnaireDocument CreateQuestionnaireDocumentWith7NumericQuestionsInDifferentRosters(Guid questionnaireId,
            Guid textAId, Guid textBId, Guid textCId, Guid textDId, Guid textEId, Guid textFId, Guid textGId, Guid textHId, Guid textIId, string textAVar, string textBVar, string textCVar, string textDVar, string textEVar, string textFVar, string textGVar, string textHVar, string textIVar)
        {
            Guid numericId = Guid.Parse("55552222111100000000111122225555");
            Guid multiId = Guid.Parse("66662222111100000000111122225555");
            Guid listId = Guid.Parse("77772222111100000000111122225555");
            return new QuestionnaireDocument
            {
                PublicKey = questionnaireId,
                Children = new List<IComposite>
                {
                    new TextQuestion("Text A") { PublicKey = textAId, StataExportCaption = textAVar, QuestionType = QuestionType.Text },
                    new Group("Roster Fixed")
                    {
                        PublicKey = Guid.NewGuid(),
                        IsRoster = true,
                        RosterSizeSource = RosterSizeSourceType.FixedTitles,
                        RosterFixedTitles = new[] { "Title1", "Title2" },
                        Children = new List<IComposite>
                        {
                            new TextQuestion("Text B") { PublicKey = textBId, StataExportCaption = textBVar, QuestionType = QuestionType.Text },
                            new Group("Group in roster")
                            {
                                PublicKey = Guid.NewGuid(),
                                Children = new List<IComposite>()
                                {
                                    new TextQuestion("Text A") { PublicKey = textCId, StataExportCaption = textCVar, QuestionType = QuestionType.Text }
                                }
                            }
                        }
                    },
                    new NumericQuestion("Numeric") { PublicKey = numericId, StataExportCaption = "numeric", QuestionType = QuestionType.Numeric, IsInteger = true },
                    new Group("Roster Numeric")
                    {
                        PublicKey = Guid.NewGuid(),
                        IsRoster = true,
                        RosterSizeSource = RosterSizeSourceType.Question,
                        RosterSizeQuestionId = numericId,
                        Children = new List<IComposite>
                        {
                            new TextQuestion("Text D") { PublicKey = textDId, StataExportCaption = textDVar, QuestionType = QuestionType.Text },
                            new Group("Group in roster")
                            {
                                PublicKey = Guid.NewGuid(),
                                Children = new List<IComposite>()
                                {
                                    new TextQuestion("Text E") { PublicKey = textEId, StataExportCaption = textEVar, QuestionType = QuestionType.Text }
                                }
                            }
                        }
                    },
                    new MultyOptionsQuestion("MultyOptions")
                    {
                        PublicKey = multiId,
                        QuestionType = QuestionType.MultyOption,
                        StataExportCaption = "multyOptions",
                        Answers = new List<Answer>()
                        {
                            new Answer { PublicKey = Guid.NewGuid(), AnswerValue = "1.5", AnswerText = "Option 1" },
                            new Answer { PublicKey = Guid.NewGuid(), AnswerValue = "2.5", AnswerText = "Option 2" }
                        }
                    },
                    new Group("Roster MultyOptions")
                    {
                        PublicKey = Guid.NewGuid(),
                        IsRoster = true,
                        RosterSizeSource = RosterSizeSourceType.Question,
                        RosterSizeQuestionId = multiId,
                        Children = new List<IComposite>
                        {
                            new TextQuestion("Text D") { PublicKey = textFId, StataExportCaption = textFVar, QuestionType = QuestionType.Text },
                            new Group("Group in roster")
                            {
                                PublicKey = Guid.NewGuid(),
                                Children = new List<IComposite>()
                                {
                                    new TextQuestion("Text E") { PublicKey = textGId, StataExportCaption = textGVar, QuestionType = QuestionType.Text }
                                }
                            }
                        }
                    },
                    new TextListQuestion("TextList") { PublicKey = listId, QuestionType = QuestionType.TextList, StataExportCaption = "textList"},
                    new Group("Roster TextList")
                    {
                        PublicKey = Guid.NewGuid(),
                        IsRoster = true,
                        RosterSizeSource = RosterSizeSourceType.Question,
                        RosterSizeQuestionId = listId,
                        Children = new List<IComposite>
                        {
                            new TextQuestion("Text F") { PublicKey = textHId, StataExportCaption = textHVar, QuestionType = QuestionType.Text },
                            new Group("Group in roster")
                            {
                                PublicKey = Guid.NewGuid(),
                                Children = new List<IComposite>()
                                {
                                    new TextQuestion("Text H") { PublicKey = textIId, StataExportCaption = textIVar, QuestionType = QuestionType.Text }
                                }
                            }
                        }
                    }
                }
            };
        }
    }
}