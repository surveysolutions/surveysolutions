using System;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities.Question;
using NUnit.Framework;
using WB.Core.BoundedContexts.Headquarters.AssignmentImport.Verifier;
using WB.Core.SharedKernels.DataCollection;
using WB.Infrastructure.Native.Storage;
using WB.UI.Headquarters.API.PublicApi.Models;
using WB.UI.Headquarters.Controllers.Api.PublicApi;
using AssignmentAnswer = WB.UI.Headquarters.Controllers.Api.PublicApi.AssignmentAnswer;

namespace WB.Tests.Web.Headquarters.Controllers;

public class AssignmentsExtensionsTests
{
    [Test]
    public void convert_to_preload_answer()
    {
        const string questionPublicKey = "11111111111111111111111111111111";
        var questionnaire = Abc.Create.Entity.PlainQuestionnaire(
            Abc.Create.Entity.QuestionnaireDocument(
                    children: new IComposite[]{ new TextListQuestion()
                    {
                        PublicKey = Guid.Parse(questionPublicKey),
                        StataExportCaption = "test1"
                    }}));

        AssignmentAnswer answer = new AssignmentAnswer(
            new AssignmentIdentifyingDataItem()
            {
                Answer = "[\"test\", \"test1\"]",
                Identity = questionPublicKey,
                Variable = "test1"
            },
            new Identity(Guid.Parse(questionPublicKey), RosterVector.Empty)) { Variable = "test1" };
        var answerValue = answer.ToPreloadAnswer(questionnaire, new NewtonJsonSerializer()) as AssignmentAnswers;

        Assert.That(answerValue?.Values.Length, Is.EqualTo(2));
    }
}
