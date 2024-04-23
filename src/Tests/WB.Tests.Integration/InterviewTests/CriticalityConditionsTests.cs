using System;
using System.Collections.Generic;
using System.Linq;
using DocumentFormat.OpenXml.Office2010.Excel;
using Ncqrs.Spec;
using NUnit.Framework;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.QuestionnaireEntities;
using WB.Core.SharedKernels.SurveySolutions.Documents;

namespace WB.Tests.Integration.InterviewTests;

[TestFixture]
public class CriticalityConditionsTests: InterviewTestsContext
{
    [Test]
    public void when_run_criticality_conditions_should_return_ids_of_fail()
    {
        var userId = Guid.Parse("11111111111111111111111111111111");

        var questionnaireId = Guid.Parse("77778888000000000000000000000000");
        var questionId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
        var variableId = Guid.Parse("BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB");

        var appDomainContext = AppDomainContext.Create();

        var results = Execute.InStandaloneAppDomain(appDomainContext.Domain, () =>
        {
            SetUp.MockedServiceLocator();

            var questionnaireDocument = Abc.Create.Entity.QuestionnaireDocumentWithOneChapter(questionnaireId,
                Abc.Create.Entity.NumericIntegerQuestion(questionId, variable: "i", validationConditions: new[]
                {
                    new ValidationCondition("v == 5", "v == 5"),
                }),
                Abc.Create.Entity.Variable(variableId, VariableType.LongInteger, "v", "i")
            );
            questionnaireDocument.CriticalityConditions.Add(new CriticalityCondition(Abc.Id.g1, "v == 6", "v == 6"));
            questionnaireDocument.CriticalityConditions.Add(new CriticalityCondition(Abc.Id.g2, "v != 6", "v != 6"));
            questionnaireDocument.CriticalityConditions.Add(new CriticalityCondition(Abc.Id.g3, "v != 1", "v != 1"));

            var interview = SetupStatefullInterviewWithExpressionStorage(appDomainContext.AssemblyLoadContext, questionnaireDocument, new List<object>());

            using (var eventContext = new EventContext())
            {
                interview.AnswerNumericIntegerQuestion(userId, questionId, RosterVector.Empty, DateTime.Now, 1);
                
                return new
                {
                    FailedCriticalityConditions = interview.CollectInvalidCriticalRules()
                };
            }
        });

        Assert.That(results, Is.Not.Null);
        Assert.That(results.FailedCriticalityConditions, Is.Not.Null);
        Assert.That(results.FailedCriticalityConditions.Count(), Is.EqualTo(2));
        Assert.That(results.FailedCriticalityConditions.First(), Is.EqualTo(Abc.Id.g1));
        Assert.That(results.FailedCriticalityConditions.Last(), Is.EqualTo(Abc.Id.g3));

        appDomainContext.Dispose();
        appDomainContext = null;
    }
    
    
    [Test]
    public void when_criticality_conditions_message_use_substitutions_should_return_correct_message()
    {
        var userId = Guid.Parse("11111111111111111111111111111111");

        var questionnaireId = Guid.Parse("77778888000000000000000000000000");
        var questionId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");

        var appDomainContext = AppDomainContext.Create();

        var results = Execute.InStandaloneAppDomain(appDomainContext.Domain, () =>
        {
            SetUp.MockedServiceLocator();

            var questionnaireDocument = Abc.Create.Entity.QuestionnaireDocumentWithOneChapter(questionnaireId,
                Abc.Create.Entity.TextQuestion(questionId, variable: "tt")
            );
            questionnaireDocument.CriticalityConditions.Add(new CriticalityCondition(Abc.Id.g1, "tt == \"test\"", "check fail: %tt%"));

            var interview = SetupStatefullInterviewWithExpressionStorage(appDomainContext.AssemblyLoadContext, questionnaireDocument, new List<object>());

            using (var eventContext = new EventContext())
            {
                interview.AnswerTextQuestion(userId, questionId, RosterVector.Empty, DateTime.Now, "true");
                
                return new
                {
                    CriticalityMessage = interview.GetCriticalRuleMessage(Abc.Id.g1)
                };
            }
        });

        Assert.That(results, Is.Not.Null);
        Assert.That(results.CriticalityMessage, Is.Not.Null);
        Assert.That(results.CriticalityMessage, Is.EqualTo("check fail: true"));

        appDomainContext.Dispose();
        appDomainContext = null;
    }
}
