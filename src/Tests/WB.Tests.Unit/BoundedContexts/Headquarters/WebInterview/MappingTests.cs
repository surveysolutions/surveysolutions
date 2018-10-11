using System;
using AutoMapper;
using Main.Core.Entities.SubEntities;
using NUnit.Framework;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.InterviewEntities.Answers;
using WB.Enumerator.Native.WebInterview.Models;
using WB.Tests.Abc;

namespace WB.Tests.Unit.BoundedContexts.Headquarters.WebInterview
{
    public class MappingToDtoTests
    {
        private IMapper mapper;

        [OneTimeSetUp]
        public void Setup()
        {
            var autoMapperConfig = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile(new WebInterviewAutoMapProfile());
            });

            mapper = autoMapperConfig.CreateMapper();
        }

        [Test]
        public void should_be_able_to_map_TextList_questions()
        {
            var question = Create.Entity.InterviewTreeQuestion(Create.Entity.Identity(), questionType: QuestionType.TextList);
            question.SetAnswer(TextListAnswer.FromTextListAnswerRows(new []
            {
                new TextListAnswerRow(0, "test"),
                new TextListAnswerRow(1, "test")
            }), new DateTime(2018, 10, 31));

            var dto = this.mapper.Map<InterviewTextListQuestion>(question);

            Assert.That(dto.Rows, Has.Count.EqualTo(2));
            Assert.That(dto.Rows[0].Text, Is.EqualTo("test"));
            Assert.That(dto.Rows[1].Value, Is.EqualTo(1).Within(0.1m));
        }
    }
}
