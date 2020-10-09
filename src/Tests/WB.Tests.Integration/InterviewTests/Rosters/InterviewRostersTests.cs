using System;
using System.Linq;
using FluentAssertions;
using Main.Core.Entities.Composite;
using Ncqrs.Spec;
using NUnit.Framework;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.Enumerator.Views;
using WB.Core.SharedKernels.QuestionnaireEntities;
using WB.Core.SharedKernels.SurveySolutions.Documents;
using WB.Tests.Abc;

namespace WB.Tests.Integration.InterviewTests.Rosters
{
    [TestFixture]
    public class InterviewRostersTests : InterviewTestsContext
    {
        [Test]
        public void when_category_multi_question_is_roster_triger_should_create_roster_instances()
        {
            var multiQuestionId = Guid.Parse("11111111111111111111111111111111");
            var rosterId = Guid.Parse("22222222222222222222222222222222");
            Guid textId = Guid.Parse("33333333333333333333333333333333");
            Guid categoryId = Guid.Parse("44444444444444444444444444444444");

            Guid userId = Guid.Parse("aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa");

            var appDomainContext = AppDomainContext.Create();

            var result = Execute.InStandaloneAppDomain(appDomainContext.Domain, () =>
            {
                SetUp.MockedServiceLocator();

                var questionnaireDocument = Create.Entity.QuestionnaireDocumentWithOneChapter(children: new IComposite[]
                {
                    Create.Entity.MultipleOptionsQuestion(multiQuestionId, categoryId: categoryId),
                    Create.Entity.MultiRoster(rosterId, rosterSizeQuestionId: multiQuestionId, children: new IComposite[]
                    {
                        Create.Entity.TextQuestion(textId),
                    })
                });
                questionnaireDocument.Categories.Add(new Categories() { Id = categoryId, Name = "name"});

                var questionnaireIdentity = new QuestionnaireIdentity(questionnaireDocument.PublicKey, 1);
                var optionsPlainStorage = Create.Storage.SqliteInmemoryStorage<OptionView, int?>(
                    Create.Entity.OptionView(questionnaireIdentity, 1, "111", null, categoryId),
                    Create.Entity.OptionView(questionnaireIdentity, 2, "222", null, categoryId),
                    Create.Entity.OptionView(questionnaireIdentity, 3, "333", null, categoryId),
                    Create.Entity.OptionView(questionnaireIdentity, 4, "444", null, categoryId),
                    Create.Entity.OptionView(questionnaireIdentity, 5, "555", null, categoryId)
                );
                var optionsRepository = Create.Storage.QuestionOptionsRepository(optionsPlainStorage); 
                var interview = SetupInterview(appDomainContext.AssemblyLoadContext, questionnaireDocument: questionnaireDocument,
                    optionsRepository);

                using var eventContext = new EventContext();
                interview.AnswerMultipleOptionsQuestion(userId, multiQuestionId, RosterVector.Empty, DateTime.Now, new[] { 1, 2, 4 });

                return new
                {
                    CreatedRosterInstances = eventContext.GetSingleEventOrNull<RosterInstancesAdded>()?.Instances,
                };
            });
            
            Assert.That(result.CreatedRosterInstances.Length, Is.EqualTo(3));
            Assert.That(result.CreatedRosterInstances[0].RosterInstanceId, Is.EqualTo(1));
            Assert.That(result.CreatedRosterInstances[1].RosterInstanceId, Is.EqualTo(2));
            Assert.That(result.CreatedRosterInstances[2].RosterInstanceId, Is.EqualTo(4));
            Assert.That(result.CreatedRosterInstances.All(i => i.GroupId == rosterId), Is.True);
            
            appDomainContext.Dispose();
            appDomainContext = null;
        }
    }
}