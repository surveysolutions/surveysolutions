using System;
using System.Collections.Generic;
using NUnit.Framework;
using WB.Core.SharedKernels.DataCollection;
using WB.Tests.Abc;

namespace WB.Tests.Integration.WebTester.Services
{
    public class when_get_first_top_filtered_categorical_options : AppdomainsPerInterviewManagerTestsBase
    {
        private Guid singleQuestionId = Id.gB;

        Guid interviewId = Guid.NewGuid();
        private Guid interviewerId = Id.g1;
        private Identity singleWithFilterQuestionId = Id.Identity9;
        private List<CategoricalOption> options;

        [OneTimeSetUp]
        public void Setup()
        {
            Manager = CreateManager();

            SetupAppDomainInterview(Manager, interviewId, Create.Entity.QuestionnaireDocumentWithOneChapter(Id.g1,
                Create.Entity.SingleOptionQuestion(singleQuestionId, variable: "sq",
                    answerCodes: new decimal[] { 1, 2, 3 }),

                Create.Entity.SingleOptionQuestion(singleWithFilterQuestionId.Id,
                    variable: "sf",
                    answerCodes: new decimal[] { 1, 2, 3 },
                    optionsFilterExpression:
                    "sq.InList(2, 3) && @optioncode!=1 || !sq.InList(2, 3) && @optioncode >= 1")
            ));

            Manager.Execute(Create.Command.AnswerSingleOptionQuestionCommand(interviewId, interviewerId, 2, singleQuestionId));
            
            this.options = Manager.GetFirstTopFilteredOptionsForQuestion(interviewId,
                singleWithFilterQuestionId, null, null, 3);
        }

        [OneTimeTearDown]
        public void TearDown()
        {
            Manager.TearDown(interviewId);
        }

        [Test]
        public void should_filter_out_options_to_two() => Assert.That(options, Has.Count.EqualTo(2));

        [Test]
        public void should_not_include_option_with_value_1() => Assert.That(options,
            Has.None.Property(nameof(CategoricalOption.Value)).EqualTo(1));

        [Test]
        public void should_include_option_with_value_2() => Assert.That(options,
            Has.One.Property(nameof(CategoricalOption.Value)).EqualTo(2));

        [Test]
        public void should_include_option_with_value_3() => Assert.That(options,
            Has.One.Property(nameof(CategoricalOption.Value)).EqualTo(3));
    }
}