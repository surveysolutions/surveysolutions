using System;
using System.Collections.Generic;
using System.Linq;
using BenchmarkDotNet.Attributes;
using Main.Core.Documents;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.SurveySolutions.Documents;

namespace PerformanceTest
{
    [Config(typeof(MonitoringConfig))]
    public class ExpressionStorageBench : ExpressionStorageBenchBase
    {
        private static readonly Guid q1Id = Guid.Parse("11111111111111111111111111111111");
        private static readonly Guid q2Id = Guid.Parse("22222222222222222222222222222222");
        private static readonly Guid q3Id = Guid.Parse("33333333333333333333333333333333");
        private static readonly Guid numericId = Guid.Parse("44444444444444444444444444444444");
        private static readonly Guid rosterId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
        private static readonly Guid questionnaireId = Guid.Parse("FFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFF");
        private static readonly Guid userId = Guid.NewGuid();

        protected override QuestionnaireDocument CreateDocument()
        {
            var multioptions = Enumerable.Range(1, 200).Select(x => Create.Option(x)).ToArray();
            var questionnaireDocument = Create.QuestionnaireDocumentWithOneChapter(questionnaireId, children: new IComposite[]
            {
                 Create.TextQuestion(q3Id, variable:"testtest"),
                 Create.MultiQuestion(q1Id, variable: "q1", options: multioptions, maxAllowedAnswers: Constants.MaxLongRosterRowCount),
                 Create.MultiRoster(rosterId, variable:"r1", enablementCondition:"single==1", sizeQuestionId: q1Id, children: GenerateChildQuestions(30, "test")),
                 Create.SingleQuestion(q2Id, variable: "single", options: new List<Answer>
                 {
                    Create.Option("1", text: "Enable roster %testtest%"),
                    Create.Option("2", text: "Disable roster %testtest%")
                 }),
                Create.NumericIntegerQuestion(numericId, "num_roster"),
                Create.NumericRoster(new Guid("99999999999999999999999999999999"), "roster_numeric", 
                numericId, GenerateChildRosterQuestions(5, "num").ToArray())
            });

            return questionnaireDocument;

            IComposite[] GenerateChildQuestions(int num, string prefix)
            {
                return Enumerable.Range(1, num).Select(x =>
                    Create.TextQuestion(variable: $"{prefix}_{x}")).ToArray();
            }

            IEnumerable<IComposite> GenerateChildRosterQuestions(int num, string prefix)
            {
                var numeric = Create.NumericIntegerQuestion(Guid.NewGuid(), "numeric_nested" + prefix);
                yield return numeric;

                foreach (var x in Enumerable.Range(1, num))
                {
                    yield return Create.NumericRoster(Guid.NewGuid(), $"nested_roster_{x}", numericId,
                        Create.TextQuestion(Guid.NewGuid(), "someText22" + x),
                        Create.GpsCoordinateQuestion(Guid.NewGuid(), "gps22"  + x)
                    );
                }
            }
        }
        
       // [Benchmark]
        public void TriggerRosterByAnswerSingleOptionQuestion()
        {
            this.interview.AnswerSingleOptionQuestion(userId, q2Id, RosterVector.Empty, DateTime.Now, 1);
            this.interview.AnswerSingleOptionQuestion(userId, q2Id, RosterVector.Empty, DateTime.Now, 2);
        }

      //  [Benchmark]
        public void AnswerSimpleTextQuestion()
        {
            this.interview.AnswerTextQuestion(userId, q3Id, RosterVector.Empty, DateTime.UtcNow, "testtest");
        }

       // [Benchmark]
        public void TriggerRosterSizeChange()
        {
            this.interview.AnswerNumericIntegerQuestion(userId, numericId, RosterVector.Empty, DateTime.UtcNow, 0);
            this.interview.AnswerNumericIntegerQuestion(userId, numericId, RosterVector.Empty, DateTime.UtcNow, 60);
        }

        [Benchmark]
        public void TriggerRosterSizeChangeUpDown()
        {
            //Console.WriteLine("Start roster trigger - 0");
            this.interview.AnswerNumericIntegerQuestion(userId, numericId, RosterVector.Empty, DateTime.UtcNow, 0);

           // Console.WriteLine("Start roster trigger - 60");
            this.interview.AnswerNumericIntegerQuestion(userId, numericId, RosterVector.Empty, DateTime.UtcNow, 60);

            // Console.WriteLine("Start roster trigger - 0");
            this.interview.AnswerNumericIntegerQuestion(userId, numericId, RosterVector.Empty, DateTime.UtcNow, 0);
        }
    }
}