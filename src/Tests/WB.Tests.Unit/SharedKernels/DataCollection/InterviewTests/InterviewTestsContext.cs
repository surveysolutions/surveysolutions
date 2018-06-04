using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Main.Core.Documents;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using NUnit.Framework;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.InterviewEntities.Answers;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.DataCollection.Services;
using WB.Tests.Abc;

namespace WB.Tests.Unit.SharedKernels.DataCollection.InterviewTests
{
    [TestOf(typeof(Interview))]
    public class InterviewTestsContext
    {
        protected static Interview CreateInterview(QuestionnaireDocument questionnaire)
        {
            var questionnaireId = Guid.NewGuid();

            var questionnaireRepository = CreateQuestionnaireRepositoryStubWithOneQuestionnaire(questionnaireId, Create.Entity.PlainQuestionnaire(questionnaire));
            return CreateInterview(questionnaireId: questionnaireId, questionnaireRepository: questionnaireRepository);
        }
        
        protected static Interview CreateInterview(Guid? interviewId = null, Guid? userId = null, Guid? questionnaireId = null,
            List<InterviewAnswer> answersToFeaturedQuestions = null, DateTime? answersTime = null, Guid? supervisorId = null,
            IQuestionnaireStorage questionnaireRepository = null, 
            IInterviewExpressionStatePrototypeProvider expressionProcessorStatePrototypeProvider = null)
        {
            var textFactory = Create.Service.SubstitutionTextFactory();

            var interview = Create.AggregateRoot.Interview(
                questionnaireRepository: questionnaireRepository,
                expressionProcessorStatePrototypeProvider: expressionProcessorStatePrototypeProvider,
                textFactory: textFactory);

            interview.CreateInterview(Create.Command.CreateInterview(interview.EventSourceId, userId ?? new Guid("F000F000F000F000F000F000F000F000"), 
                questionnaireId ?? new Guid("B000B000B000B000B000B000B000B000"), 1,
                answersToFeaturedQuestions ?? new List<InterviewAnswer>(),  
                supervisorId ?? new Guid("D222D222D222D222D222D222D222D222"), null, Create.Entity.InterviewKey()));

            return interview;
        }

        protected static IQuestionnaireStorage CreateQuestionnaireRepositoryStubWithOneQuestionnaire(Guid questionnaireId, IQuestionnaire questionaire = null)
        {
            return Create.Fake.QuestionnaireRepositoryWithOneQuestionnaire(questionnaireId, questionaire);
        }

        protected static QuestionnaireDocument CreateQuestionnaireDocumentWithOneChapter(params IComposite[] children)
        {
            var result = new QuestionnaireDocument
            {
                Children = new IComposite[]
                {
                    new Group("Chapter")
                    {
                        Children = children?.ToReadOnlyCollection() ?? new ReadOnlyCollection<IComposite>(new List<IComposite>())
                    }
                }.ToReadOnlyCollection()
            };
            
            return result;
        }
    }
}
