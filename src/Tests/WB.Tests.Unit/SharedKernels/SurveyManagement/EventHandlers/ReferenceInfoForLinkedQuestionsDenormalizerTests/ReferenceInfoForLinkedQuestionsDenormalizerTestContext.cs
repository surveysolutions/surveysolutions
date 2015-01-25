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
using WB.Core.SharedKernels.DataCollection.Implementation.Factories;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.SurveyManagement.EventHandler;
using WB.Core.SharedKernels.SurveyManagement.Implementation.Factories;
using WB.Core.SharedKernels.SurveyManagement.Views.Questionnaire;

using QuestionnaireDeleted = WB.Core.SharedKernels.DataCollection.Events.Questionnaire.QuestionnaireDeleted;
using TemplateImported = datacollection::Main.Core.Events.Questionnaire.TemplateImported;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.EventHandlers.ReferenceInfoForLinkedQuestionsDenormalizerTests
{
    internal class ReferenceInfoForLinkedQuestionsDenormalizerTestContext
    {
        protected static IPublishedEvent<T> ToPublishedEvent<T>(T @event, Guid? eventSourceId = null) where T : class
        {
            return Mock.Of<IPublishedEvent<T>>(publishedEvent => publishedEvent.Payload == @event && publishedEvent.EventSourceId == (eventSourceId ?? Guid.Parse("33333333333333333333333333333333")));
        }

        protected static IPublishedEvent<TemplateImported> CreateTemplateImportedEvent(QuestionnaireDocument questionnaire = null, long? version=null)
        {
            var questionnaireDocument = questionnaire ?? new QuestionnaireDocument();
            return ToPublishedEvent(new TemplateImported
            {
                Source = questionnaireDocument,
                Version = version
            },
                questionnaireDocument.PublicKey);
        }


        protected static IPublishedEvent<QuestionnaireDeleted> CreateQuestionnaireDeletedEvent(Guid questionnaireId, long version)
        {
            return ToPublishedEvent(new QuestionnaireDeleted
            {
                QuestionnaireVersion = version
            },
                questionnaireId);
        }

        protected static ReferenceInfoForLinkedQuestionsDenormalizer CreateReferenceInfoForLinkedQuestionsDenormalizer(
            IReadSideKeyValueStorage<ReferenceInfoForLinkedQuestions> questionnaires = null)
        {
            return
                new ReferenceInfoForLinkedQuestionsDenormalizer(questionnaires ??
                    Mock.Of<IReadSideKeyValueStorage<ReferenceInfoForLinkedQuestions>>(),
                    new ReferenceInfoForLinkedQuestionsFactory(),
                    Mock.Of<IPlainQuestionnaireRepository>());
        }

        protected static QuestionnaireDocument CreateQuestionnaireDocumentWithRosterAndNumericQuestionAndLinedQuestionAfter(Guid questionnaireId, Guid rosterId, Guid linkedId)
        {
            Guid numericId = Guid.Parse("55552222111100000000111122225555");
            return new QuestionnaireDocument
            {
                PublicKey = questionnaireId,
                Children = new List<IComposite>
                {
                    new Group("Roster")
                    {
                        PublicKey = rosterId,
                        IsRoster = true,
                        RosterSizeSource = RosterSizeSourceType.FixedTitles,
                        RosterFixedTitles = new []{"Title1", "Title2"},
                        Children = new List<IComposite>
                        {
                            new NumericQuestion("Numeric")
                            {
                                PublicKey = numericId,
                                QuestionType = QuestionType.Numeric,
                                IsInteger = true
                            }
                        }
                    },
                    new MultyOptionsQuestion("MultyOption")
                    {
                        PublicKey = linkedId,
                        QuestionType = QuestionType.MultyOption,
                        LinkedToQuestionId = numericId
                    }
                }
            };
        }
    }
}