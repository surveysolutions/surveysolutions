using System;
using System.Collections.Generic;
using Main.Core.Documents;
using Main.Core.Entities.SubEntities;
using NUnit.Framework;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Portable.ServiceLocation;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Tests.Abc;

namespace WB.Tests.Integration.InterviewTests.EnablementAndValidness
{
    [TestOf(typeof(Interview))]
    internal class when_answer_on_cascading_question_used_in_conditions : InterviewTestsContext
    {
        [SetUp]
        public void Context()
        {
            QuestionnaireDocument questionnaire = Create.Entity.QuestionnaireDocumentWithOneChapter(
                Create.Entity.SingleOptionQuestion(questionId: parentQuestionId, variable: "parent", answers: new List<Answer>()
                {
                    new Answer() {AnswerCode = 1, AnswerText = "1", AnswerValue = "1"},
                    new Answer() {AnswerCode = 2, AnswerText = "2", AnswerValue = "2"}
                }),
                Create.Entity.SingleOptionQuestion(questionId: cascadingQuestionId, variable: "casc",cascadeFromQuestionId: parentQuestionId, 
                answers: new List<Answer>()
                {
                    new Answer()
                    {
                        AnswerCode = 1,
                        AnswerText = "1",
                        AnswerValue = "1",
                        ParentCode = 1,
                        ParentValue = "1"
                    },
                    new Answer()
                    {
                        AnswerCode = 2,
                        AnswerText = "2",
                        AnswerValue = "2",
                        ParentCode = 2,
                        ParentValue = "2"
                    }
                }),
                Create.Entity.NumericQuestion(questionId: numericQuestionId, variableName: "num", enablementCondition:"IsAnswered(casc)")
            );

            var optionsRepo = Moq.Mock.Of<IQuestionOptionsRepository>(x =>
                x.GetOptionForQuestionByOptionValue(Moq.It.IsAny<QuestionnaireIdentity>(), cascadingQuestionId, 1, Moq.It.IsAny<Core.SharedKernels.SurveySolutions.Documents.Translation>()) == new CategoricalOption
                {
                    Value = 1,
                    ParentValue = 1,
                    Title = "1"
                }
                && x.GetOptionsForQuestion(Moq.It.IsAny<QuestionnaireIdentity>(), cascadingQuestionId, 1, "", Moq.It.IsAny<Core.SharedKernels.SurveySolutions.Documents.Translation>()) == new CategoricalOption
                {
                    Value = 1,
                    ParentValue = 1,
                    Title = "1"
                }.ToEnumerable()
            );

            Moq.Mock.Get(ServiceLocator.Current).Setup(_ => _.GetInstance<IQuestionOptionsRepository>()).Returns(optionsRepo);

            interview = SetupStatefullInterview(questionnaire);

            interview.AnswerSingleOptionQuestion(userId, parentQuestionId, RosterVector.Empty, DateTime.Now, 1);
            interview.AnswerSingleOptionQuestion(userId, cascadingQuestionId, RosterVector.Empty, DateTime.Now, 1);
            
        }

        [TearDown]
        public void Cleanup ()
        {
            interview = null;
        }

        [Test]
        public void should_enable_dependent_question()
        {
            Assert.That(interview.GetQuestion(Identity.Create(numericQuestionId, RosterVector.Empty)).IsDisabled(), Is.False);
        }

        
        private static StatefulInterview interview;
        private static readonly Guid userId = Guid.Parse("FFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFF");
        private static readonly Guid cascadingQuestionId = Guid.Parse("33333333333333333333333333333333");
        private static readonly Guid parentQuestionId = Guid.Parse("22222222222222222222222222222222");
        private static readonly Guid numericQuestionId = Guid.Parse("11111111111111111111111111111111");
    }
}
