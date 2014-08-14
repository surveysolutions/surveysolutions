﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Machine.Specifications;
using Main.Core.Documents;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using Microsoft.Practices.ServiceLocation;
using Moq;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;
using WB.Core.SharedKernels.DataCollection.Implementation.Repositories;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.DataCollection.Utils;
using WB.Core.SharedKernels.ExpressionProcessor.Services;

namespace WB.Tests.Integration.InterviewTests
{
    [Subject(typeof(Interview))]
    internal class InterviewTestsContext
    {
        protected static Interview CreateInterviewFromQuestionnaireDocumentRegisteringAllNeededDependencies(QuestionnaireDocument questionnaireDocument)
        {
            var questionnaireId = Guid.Parse("10000010000100100100100001000001");

            Questionnaire questionnaire = CreateQuestionnaire(questionnaireDocument);

            var questionnaireRepository = CreateQuestionnaireRepositoryStubWithOneQuestionnaire(questionnaireId, questionnaire);

            Mock.Get(ServiceLocator.Current)
                .Setup(locator => locator.GetInstance<IQuestionnaireRepository>())
                .Returns(questionnaireRepository);

            return CreateInterview(questionnaireId: questionnaireId);
        }

        protected static Interview CreateInterview(Guid? interviewId = null, Guid? userId = null, Guid? questionnaireId = null,
            Dictionary<Guid, object> answersToFeaturedQuestions = null, DateTime? answersTime = null, Guid? supervisorId = null)
        {
            return new Interview(
                interviewId ?? new Guid("A0A0A0A0B0B0B0B0A0A0A0A0B0B0B0B0"),
                userId ?? new Guid("F111F111F111F111F111F111F111F111"),
                questionnaireId ?? new Guid("B000B000B000B000B000B000B000B000"),1,
                answersToFeaturedQuestions ?? new Dictionary<Guid, object>(),
                answersTime ?? new DateTime(2012, 12, 20),
                supervisorId ?? new Guid("D222D222D222D222D222D222D222D222"));
        }

        protected static Questionnaire CreateQuestionnaire(QuestionnaireDocument questionnaireDocument, Guid? userId = null)
        {
            return new Questionnaire(
                userId ?? new Guid("E333E333E333E333E333E333E333E333"),
                questionnaireDocument, false);
        }

        protected static IQuestionnaireRepository CreateQuestionnaireRepositoryStubWithOneQuestionnaire(Guid questionnaireId, IQuestionnaire questionaire)
        {
            return Mock.Of<IQuestionnaireRepository>(repository
                => repository.GetQuestionnaire(questionnaireId) == questionaire
                && repository.GetHistoricalQuestionnaire(questionnaireId, Moq.It.IsAny<long>()) == questionaire);
        }

        protected static QuestionnaireDocument CreateQuestionnaireDocumentWithOneChapter(params IComposite[] children)
        {
            var result = new QuestionnaireDocument();
            var chapter = new Group("Chapter");
            result.Children.Add(chapter);

            foreach (var child in children)
            {
                chapter.Children.Add(child);
            }

            return result;
        }

        protected static void SetupInstanceToMockedServiceLocator<TInstance>(TInstance instance)
        {
            Mock.Get(ServiceLocator.Current)
                .Setup(locator => locator.GetInstance<TInstance>())
                .Returns(instance);
        }
    }
}
